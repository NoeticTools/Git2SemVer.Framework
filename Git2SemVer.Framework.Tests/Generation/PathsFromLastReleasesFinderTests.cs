﻿using Moq;
using NoeticTools.Git2SemVer.Core.Logging;
using NoeticTools.Git2SemVer.Core.Tools.Git;
using NoeticTools.Git2SemVer.Framework.Generation;
using NoeticTools.Git2SemVer.Framework.Tests.TestScenarios;
using NoeticTools.Git2SemVer.Testing.Core;


#pragma warning disable NUnit2045

namespace NoeticTools.Git2SemVer.Framework.Tests.Generation;

[TestFixture]
internal class PathsFromLastReleasesFinderTests
{
    private Dictionary<string, Commit> _commitsLookup;
    private Mock<IGitTool> _gitTool;
    private NUnitLogger _logger;
    private Mock<ICommitsCache> _repository;

    [SetUp]
    public void SetUp()
    {
        _logger = new NUnitLogger(false)
        {
            Level = LoggingLevel.Debug
        };
        _commitsLookup = new Dictionary<string, Commit>();
        _repository = new Mock<ICommitsCache>();
        _gitTool = new Mock<IGitTool>();
        _gitTool.Setup(x => x.Get(It.IsAny<CommitId>()))
                .Returns((CommitId commitId) => _commitsLookup[commitId.Sha]);
        _gitTool.Setup(x => x.BranchName)
                .Returns("master");
    }

    [TearDown]
    public void TearDown()
    {
        _logger.Dispose();
    }

    [TestCaseSource(typeof(ManufacturedGitRepositoriesTestSource))]
    public void FindPathsToHead(string name, GitTestRepository scenario)
    {
        _logger.LogInfo(scenario.Description + "\n");
        LoadRepository(scenario.Commits, scenario.HeadCommitId);
        var target = new PathsFromLastReleasesFinder(_gitTool.Object,
                                                     _logger);

        var historyPaths = target.FindPathsToHead();

        Assert.That(historyPaths.Paths.Count, Is.EqualTo(scenario.ExpectedPathCount));
        Assert.That(historyPaths.BestPath.Version.ToString(), Is.EqualTo(scenario.ExpectedVersion));
    }

    private void LoadRepository(IEnumerable<Commit> commits, string headCommitId)
    {
        foreach (var commit in commits)
        {
            _commitsLookup.Add(commit.CommitId.Sha, commit);
        }

        _repository.Setup(x => x.Get(It.IsAny<CommitId>())).Returns<CommitId>(commitId => _commitsLookup[commitId.Sha]);
        _gitTool.Setup(x => x.Head).Returns(_commitsLookup[headCommitId]);
    }
}