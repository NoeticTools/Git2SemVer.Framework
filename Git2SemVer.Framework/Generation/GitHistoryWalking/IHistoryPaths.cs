using System.Collections.Immutable;
using NoeticTools.Git2SemVer.Core.Tools.Git;


namespace NoeticTools.Git2SemVer.Framework.Generation.GitHistoryWalking;

public interface IHistoryPaths
{
    /// <summary>
    /// The best path found for semmantic versioning.
    /// </summary>
    /// <remarks>
    /// Returns the shortest path that results in the highest version from
    /// the prior highest released commit tag.
    /// </remarks>
    IVersionHistoryPath BestPath { get; }

    /// <summary>
    /// The head commit used.
    /// </summary>
    Commit HeadCommit { get; }

    /// <summary>
    /// All paths found to the prior highest released commit tag.
    /// </summary>
    ImmutableSortedSet<IVersionHistoryPath> Paths { get; }
}