using System.Diagnostics;
using System.Text;
using NoeticTools.Git2SemVer.Core.Logging;
using NoeticTools.Git2SemVer.Core.Tools.Git;
using Semver;


namespace NoeticTools.Git2SemVer.Framework.Generation.GitHistoryWalking;

internal sealed class VersionHistorySegmentsBuilder : IVersionHistorySegmentsBuilder
{
    private readonly Dictionary<CommitId, VersionHistorySegment> _commitsCache = new();
    private readonly IGitTool _gitTool;
    private readonly ILogger _logger;
    private readonly VersionHistorySegment _segment;
    private readonly VersionHistorySegmentFactory _segmentFactory;
    private readonly Dictionary<int, VersionHistorySegment> _segments = [];
    private SemVersion _ignoredReleaseVersion = new SemVersion(0);

    private VersionHistorySegmentsBuilder(VersionHistorySegment segment, VersionHistorySegmentsBuilder parent)
    {
        _logger = parent._logger;
        _segments = parent._segments;
        _segmentFactory = parent._segmentFactory;
        _gitTool = parent._gitTool;
        _commitsCache = parent._commitsCache;
        _segment = segment;
        _segments.Add(segment.Id, segment);
    }

    public VersionHistorySegmentsBuilder(IGitTool gitTool, ILogger logger)
    {
        _gitTool = gitTool;
        _logger = logger;
        _segmentFactory = new VersionHistorySegmentFactory(logger);
        _segment = _segmentFactory.Create();
        _segments.Add(_segment.Id, _segment);
    }

    /// <summary>
    /// Get git history path segments from prior release tags to the given commit.
    /// </summary>
    public IReadOnlyList<VersionHistorySegment> BuildTo(Commit commit, bool forcePriorRelease = false)
    {
        _logger.LogDebug("");
        _logger.LogDebug("Build git path segments from prior releases.\n");

        using (_logger.EnterLogScope())
        {
            var stopwatch = Stopwatch.StartNew();

            _ignoredReleaseVersion = forcePriorRelease && commit.ReleasedVersion != null ? commit.ReleasedVersion : new SemVersion(0);
            if (_ignoredReleaseVersion != null)
            {
                _logger.LogTrace("Ignoring release {0}.", _ignoredReleaseVersion);
            }

            BuildSegmentsTo(commit);
            LogFoundSegments();
            stopwatch.Stop();

            _logger.LogTrace($"Git path segments build ({stopwatch.ElapsedMilliseconds}ms).");
        }

        return _segments.Values.ToList();
    }

    private void LogFoundSegments()
    {
        if (_logger.Level < LoggingLevel.Debug)
        {
            return;
        }

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"Found {_segments.Count} segments:");

        stringBuilder.AppendLine("  Segment #      From -> To       Commits  Bumps  Release");
        foreach (var segment in _segments)
        {
            stringBuilder.AppendLine("  " + segment.Value);
        }

        _logger.LogDebug(stringBuilder.ToString());
    }

    private void BuildSegmentsTo(Commit commit)
    {
        while (NextCommit(commit) == SegmentWalkResult.Continue)
        {
            commit = _gitTool.Get(commit.Parents.First());
        }
    }

    private SegmentWalkResult NextCommit(Commit commit)
    {
        if (_commitsCache.ContainsKey(commit.CommitId))
        {
            OnBranchFromExistingSegment(commit);
            return SegmentWalkResult.FoundStart;
        }

        _segment.Append(commit);
        _commitsCache.Add(commit.CommitId, _segment);

        if (commit.ReleasedVersion != null && !commit.ReleasedVersion.Equals(_ignoredReleaseVersion))
        {
            using (_logger.EnterLogScope())
            {
                _logger.LogTrace("Found a prior release: Commit {0}, release tag '{1}'.", commit.CommitId.ShortSha, commit.ReleasedVersion.ToString());
            }

            return SegmentWalkResult.FoundStart;
        }

        var parents = commit.Parents.ToList();
        if (parents.Count == 0)
        {
            _logger.LogTrace("Commit {0} is the first (initial) commit in the repository (defaults to 0.1.0).", commit.CommitId.ShortSha);
            return SegmentWalkResult.FoundStart;
        }

        if (parents.Count != 2)
        {
            return SegmentWalkResult.Continue;
        }

        OnMergeCommit(commit);
        return SegmentWalkResult.FoundStart;
    }

    private void NextCommitBeforeMerge(CommitId parent, Commit mergeCommit)
    {
        var parentCommit = _gitTool.Get(parent);

        if (_commitsCache.ContainsKey(parentCommit.CommitId))
        {
            OnBranchFromExistingSegment(parentCommit);
        }
        else
        {
            using (_logger.EnterLogScope())
            {
                var newSegmentVisitor = new VersionHistorySegmentsBuilder(_segmentFactory.Create([]), this);
                newSegmentVisitor.BuildSegmentsTo(parentCommit);
            }
        }
    }

    private void OnBranchFromExistingSegment(Commit commit)
    {
        var intersectingSegment = _commitsCache[commit.CommitId];
        var branchedFromSegment = intersectingSegment.BranchesFrom(_segment, commit, _segmentFactory);
        if (branchedFromSegment == null)
        {
            return;
        }

        _segments.Add(branchedFromSegment.Id, branchedFromSegment);
        foreach (var segmentCommit in branchedFromSegment.Commits)
        {
            _commitsCache[segmentCommit.CommitId] = branchedFromSegment;
        }
    }

    private void OnMergeCommit(Commit mergeCommit)
    {
        var mergedBranchCommit = mergeCommit.Parents[0];
        var continuingBranchCommit = mergeCommit.Parents[1];
        using (_logger.EnterLogScope())
        {
            _logger.LogTrace($"Commit {mergeCommit.CommitId.ShortSha} is a merge commit.");
        }

        _logger.LogTrace("Continuing branch:");
        NextCommitBeforeMerge(continuingBranchCommit, mergeCommit);

        _logger.LogTrace($"Commit {mergeCommit.CommitId.ShortSha} is a merge commit from branch commit {mergedBranchCommit.ShortSha}:");
        using (_logger.EnterLogScope())
        {
            NextCommitBeforeMerge(mergedBranchCommit, mergeCommit);
        }
    }

    private enum SegmentWalkResult
    {
        Unknown = 0,
        Continue,
        FoundStart
    }
}