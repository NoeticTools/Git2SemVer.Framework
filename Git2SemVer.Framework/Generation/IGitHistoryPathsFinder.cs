using NoeticTools.Git2SemVer.Framework.Generation.GitHistoryWalking;


namespace NoeticTools.Git2SemVer.Framework.Generation;

internal interface IGitHistoryPathsFinder
{
    /// <summary>
    /// Get the git commit paths from the highest prior release to the current head.
    /// </summary>
    HistoryPaths GetPathsToHeadFromPriorRelease();
}