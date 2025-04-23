using System.Diagnostics.CodeAnalysis;
using NoeticTools.Git2SemVer.Core.Logging;
using NoeticTools.Git2SemVer.Core.Tools.Git;
using NoeticTools.Git2SemVer.Framework.Generation;


namespace NoeticTools.Git2SemVer.Framework;

[ExcludeFromCodeCoverage]
public sealed class GitHistoryPathsFinderFactory
{
    private readonly ILogger _logger;

    public GitHistoryPathsFinderFactory(ILogger logger)
    {
        _logger = logger;
    }

    public IGitHistoryPathsFinder Create(string workingDirectory)
    {
        var commitsRepo = new CommitsCache();
        var gitProcessCli = new GitProcessCli(_logger) { WorkingDirectory = workingDirectory };
        var gitTool = new GitTool(commitsRepo, gitProcessCli, _logger);
        return new PathsFromLastReleaseFinder(gitTool, _logger);
    }
}