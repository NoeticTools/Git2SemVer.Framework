using NoeticTools.Git2SemVer.Versioning.Generation.GitHistoryWalking;


namespace NoeticTools.Git2SemVer.Versioning.Generation;

internal interface IGitHistoryPathsFinder
{
    HistoryPaths FindPathsToHead();
}