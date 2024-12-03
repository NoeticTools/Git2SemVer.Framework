using NoeticTools.Git2SemVer.Core.Logging;
using NoeticTools.Git2SemVer.Core.Tools.Git;
using NoeticTools.Git2SemVer.Framework.Generation.GitHistoryWalking;
using System.Diagnostics;


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

// ReSharper disable CanSimplifyDictionaryLookupWithTryAdd

namespace NoeticTools.Git2SemVer.Framework.Generation;

internal sealed class PathsFromLastReleasesFinder(IGitTool gitTool, ILogger logger) : IGitHistoryPathsFinder
{
    public HistoryPaths FindPathsToHead()
    {
        logger.LogDebug("");
        logger.LogDebug("Git history walking.\n");
        using (logger.EnterLogScope())
        {
            var stopwatch = Stopwatch.StartNew();
            logger.LogDebug($"Current branch: {gitTool.BranchName}");
            var segments = new VersionHistorySegmentsBuilder(gitTool, logger).BuildTo(gitTool.Head);
            var paths = new VersionHistoryPathsBuilder(segments, logger).Build();
            stopwatch.Stop();
            logger.LogDebug("");
            logger.LogDebug($"Git history walking completed. Took {stopwatch.ElapsedMilliseconds}ms.");
            return paths;
        }
    }
}