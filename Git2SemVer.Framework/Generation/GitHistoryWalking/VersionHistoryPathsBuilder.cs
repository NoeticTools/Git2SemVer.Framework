using System.Diagnostics;
using System.Text;
using NoeticTools.Git2SemVer.Core.Logging;
using NoeticTools.Git2SemVer.Core.Tools.Git;


namespace NoeticTools.Git2SemVer.Framework.Generation.GitHistoryWalking;

#pragma warning disable CS1591
internal sealed class VersionHistoryPathsBuilder
{
    private readonly ILookup<VersionHistorySegment, VersionHistorySegment> _childSegmentsLookup;
    private readonly ILogger _logger;
    private readonly IReadOnlyList<VersionHistorySegment> _segments;
    private readonly IReadOnlyList<VersionHistorySegment> _startSegments;

    public VersionHistoryPathsBuilder(IReadOnlyList<VersionHistorySegment> segments, ILogger logger)
    {
        _segments = segments;
        _logger = logger;

        var segmentsByYoungestCommit = _segments.ToDictionary(k => k.YoungestCommit.CommitId, v => v);
        _childSegmentsLookup = GetChildSegmentsLookup(segments, segmentsByYoungestCommit);
        _startSegments = segments.Where(x => x.ParentCommits.Count == 0 ||
                                             x.TaggedReleasedVersion != null).ToList();
    }

    public HistoryPaths Build()
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogDebug("Building git paths to last releases from segments.");
        var foundPaths = FindPaths();
        stopwatch.Stop();
        var paths = new HistoryPaths(foundPaths, _segments);

        LogFoundPaths(paths, stopwatch.Elapsed);

        return paths;
    }

    private List<VersionHistoryPath> FindPaths()
    {
        var paths = new List<VersionHistoryPath>();
        foreach (var startSegment in _startSegments)
        {
            paths.AddRange(GetChildPaths(startSegment, new VersionHistoryPath(startSegment)));
        }

        var nextPathId = 1;
        foreach (var path in paths)
        {
            path.Id = nextPathId++;
        }

        return paths;
    }

    private List<VersionHistoryPath> GetChildPaths(VersionHistorySegment parentSegment, VersionHistoryPath path)
    {
        var childSegments = _childSegmentsLookup[parentSegment].ToList();
        if (childSegments.Count == 0)
        {
            return [path];
        }

        var pathSegments = new List<VersionHistoryPath>();
        foreach (var childSegment in childSegments)
        {
            pathSegments.AddRange(GetChildPaths(childSegment, path.With(childSegment)));
        }

        return pathSegments;
    }

    private static ILookup<VersionHistorySegment, VersionHistorySegment> GetChildSegmentsLookup(IReadOnlyList<VersionHistorySegment> segments,
                                                                                                Dictionary<CommitId, VersionHistorySegment>
                                                                                                    segmentsByYoungestCommit)
    {
        var childLinks = new List<(VersionHistorySegment parent, VersionHistorySegment child)>();
        foreach (var segment in segments)
        {
            foreach (var parentCommit in segment.ParentCommits)
            {
                if (segmentsByYoungestCommit.TryGetValue(parentCommit, out var parentSegment))
                {
                    childLinks.Add((parent: parentSegment, child: segment));
                }
            }
        }

        var childSegmentsLookup = childLinks.ToLookup(k => k.parent, v => v.child);
        return childSegmentsLookup;
    }

    private void LogFoundPaths(HistoryPaths paths, TimeSpan timeTaken)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"  Found {paths.Paths.Count} paths ({timeTaken.Milliseconds}ms):");
        using (_logger.EnterLogScope())
        {
            stringBuilder.AppendLine("    Path #   Segments             Commits   Bumps       From -> To");
            foreach (var path in paths.Paths)
            {
                stringBuilder.AppendLine("    " + path.ToString());
            }
        }

        stringBuilder.AppendLine();
        stringBuilder.AppendLine($"  Path {paths.BestPath.Id} will be used for versioning. Gives version: {paths.BestPath.Version}.");

        _logger.LogDebug("");
        _logger.LogDebug(stringBuilder.ToString());
    }
}