using NoeticTools.Git2SemVer.Framework.Generation.GitHistoryWalking;


namespace NoeticTools.Git2SemVer.Framework.Generation;

internal interface IGitHistoryPathsFinder
{
    HistoryPaths FindPathsToHead();
}