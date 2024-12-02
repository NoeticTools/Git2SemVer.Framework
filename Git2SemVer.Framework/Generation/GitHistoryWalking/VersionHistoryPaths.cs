using System.Collections.Immutable;
using NoeticTools.Git2SemVer.Core.Tools.Git;


namespace NoeticTools.Git2SemVer.Framework.Generation.GitHistoryWalking;

internal sealed class HistoryPaths : IHistoryPaths
{
    private readonly IReadOnlyList<VersionHistorySegment> _segments;

    public HistoryPaths(IReadOnlyList<VersionHistoryPath> paths,
                        IReadOnlyList<VersionHistorySegment> segments)
    {
        _segments = segments;
        Paths = paths.ToSortedSet();
        BestPath = Paths.First();
        HeadCommit = BestPath.HeadCommit;
    }

    public IVersionHistoryPath BestPath { get; }

    public Commit HeadCommit { get; }

    public ImmutableSortedSet<IVersionHistoryPath> Paths { get; }

    private int NumberOfCommits => _segments.Sum(x => x.Commits.Count);
}