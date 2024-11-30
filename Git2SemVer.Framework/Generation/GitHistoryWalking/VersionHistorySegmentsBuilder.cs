﻿using System.Diagnostics;
using NoeticTools.Git2SemVer.Core.Logging;
using NoeticTools.Git2SemVer.Core.Tools.Git;


namespace NoeticTools.Git2SemVer.Versioning.Generation.GitHistoryWalking;

internal sealed class VersionHistorySegmentsBuilder
{
    private readonly Dictionary<CommitId, VersionHistorySegment> _commitsCache = new();
    private readonly IGitTool _gitTool;
    private readonly ILogger _logger;
    private readonly VersionHistorySegment _segment;
    private readonly VersionHistorySegmentFactory _segmentFactory;
    private readonly Dictionary<int, VersionHistorySegment> _segments = [];

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

    public IReadOnlyList<VersionHistorySegment> BuildTo(Commit commit)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogDebug("Finding git path segments to last releases.");
        BuildSegmentsTo(commit);

        stopwatch.Stop();
        _logger.LogDebug($"Found {_segments.Count} segments ({stopwatch.ElapsedMilliseconds}ms)");

        using (_logger.EnterLogScope())
        {
            _logger.LogDebug("Segment #   Commits (count)      Bumps   Release");
            foreach (var segment in _segments)
            {
                _logger.LogDebug(segment.Value.ToString());
            }
        }

        return _segments.Values.ToList();
    }

    private void BuildSegmentsTo(Commit commit)
    {
        using (_logger.EnterLogScope())
        {
            while (NextCommit(commit) == SegmentWalkResult.Continue)
            {
                commit = _gitTool.Get(commit.Parents.First());
            }
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

        if (commit.ReleasedVersion != null)
        {
            using (_logger.EnterLogScope())
            {
                _logger.LogTrace("Commit {0} has release tag '{1}'.", commit.CommitId.Sha, commit.ReleasedVersion.ToString());
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
            var newSegmentVisitor = new VersionHistorySegmentsBuilder(_segmentFactory.Create([]), this);
            newSegmentVisitor.BuildSegmentsTo(parentCommit);
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
            _logger.LogDebug($"Commit {mergeCommit.CommitId.ShortSha} is a merge commit.");
        }

        _logger.LogTrace("Continuing branch:");
        NextCommitBeforeMerge(continuingBranchCommit, mergeCommit);

        _logger.LogDebug($"Commit {mergeCommit.CommitId.ShortSha} is a merge commit from branch commit {mergedBranchCommit.ShortSha}:");
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