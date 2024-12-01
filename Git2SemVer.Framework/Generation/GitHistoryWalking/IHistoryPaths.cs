using System.Collections.Immutable;
using NoeticTools.Git2SemVer.Core.Tools.Git;


namespace NoeticTools.Git2SemVer.Framework.Generation.GitHistoryWalking;

public interface IHistoryPaths
{
    IVersionHistoryPath BestPath { get; }

    Commit HeadCommit { get; }

    ImmutableSortedSet<IVersionHistoryPath> Paths { get; }

    string GetReport();
}