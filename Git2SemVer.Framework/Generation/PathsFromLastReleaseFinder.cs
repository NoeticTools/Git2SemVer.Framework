using NoeticTools.Git2SemVer.Core.Logging;
using NoeticTools.Git2SemVer.Core.Tools.Git;
using NoeticTools.Git2SemVer.Framework.Generation.GitHistoryWalking;
using System.Diagnostics;


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

// ReSharper disable CanSimplifyDictionaryLookupWithTryAdd

namespace NoeticTools.Git2SemVer.Framework.Generation;

internal sealed class PathsFromLastReleaseFinder(IGitTool gitTool, ILogger logger) : IGitHistoryPathsFinder
{
    public IHistoryPaths GetPathsToHeadFromPriorRelease()
    {
        logger.LogDebug("");
        logger.LogDebug("Walk Git history from head to last release.\n");
        logger.LogDebug($"Current branch: {gitTool.BranchName}");
        return GetPathsToCommitFromPriorRelease(gitTool.Head);
    }

    private HistoryPaths GetPathsToCommitFromPriorRelease(Commit toCommit)
    {
        using (logger.EnterLogScope())
        {
            var stopwatch = Stopwatch.StartNew();
            var segments = new VersionHistorySegmentsBuilder(gitTool, logger).BuildTo(toCommit);
            var paths = new VersionHistoryPathsBuilder(segments, logger).Build();
            stopwatch.Stop();
            logger.LogDebug("");
            logger.LogDebug($"Git history walking completed. Took {stopwatch.ElapsedMilliseconds}ms.");
            return paths;
        }
    }
}