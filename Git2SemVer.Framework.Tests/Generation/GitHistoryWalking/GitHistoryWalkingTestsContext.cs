using Moq;
using NoeticTools.Git2SemVer.Core.ConventionCommits;
using NoeticTools.Git2SemVer.Core.Logging;
using NoeticTools.Git2SemVer.Core.Tools.Git;
using NoeticTools.Git2SemVer.Core.Tools.Git.Parsers;
using NoeticTools.Git2SemVer.Testing.Core;


namespace NoeticTools.Git2SemVer.Framework.Tests.Generation.GitHistoryWalking;

internal class GitHistoryWalkingTestsContext : IDisposable
{
    private readonly GitLogCommitParser _logParser;

    public GitHistoryWalkingTestsContext()
    {
        Logger = new NUnitLogger(false) { Level = LoggingLevel.Trace };
        _logParser = new GitLogCommitParser(new CommitsCache(), new ConventionalCommitsParser());
        GitTool = new Mock<IGitTool>();
        GitTool.Setup(x => x.BranchName).Returns("BranchName");
    }

    public Mock<IGitTool> GitTool { get; }

    private List<Commit> GetCommits(string gitLog)
    {
        var lines = gitLog.Split('\n');
        return lines.Select(line => _logParser.Parse(line)).OfType<Commit>().ToList();
    }

    public NUnitLogger Logger { get; }

    public void SetupGitRepository(LoggedScenario scenario)
    {
        var commits = GetCommits(scenario.ActualGitLog).ToDictionary(k => k.CommitId.Sha, v => v);
        GitTool.Setup(x => x.Get(It.IsAny<CommitId>())).Returns<CommitId>(id => commits[id.Sha]);
        GitTool.Setup(x => x.Head).Returns(commits[scenario.HeadCommitId]);
    }

    public void Dispose()
    {
        Logger.Dispose();
    }
}