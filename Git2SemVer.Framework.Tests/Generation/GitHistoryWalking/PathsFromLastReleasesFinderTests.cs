using NoeticTools.Git2SemVer.Versioning.Generation;


//#pragma warning disable NUnit2045

namespace NoeticTools.Git2SemVer.Framework.Tests.Generation.GitHistoryWalking;

[TestFixture]
[Parallelizable(ParallelScope.All)]
internal class PathsFromLastReleasesFinderTests
{
    [TestCaseSource(typeof(ScenariosFromBuildLogsTestSource))]
    public void BasicScenariosTest(string name, LoggedScenario scenario)
    {
        using var context = new GitHistoryWalkingTestsContext();
        context.SetupGitRepository(scenario);
        var target = new PathsFromLastReleasesFinder(context.GitTool.Object, context.Logger);

        var paths = target.FindPathsToHead();

        var bestPath = paths.BestPath;
        Assert.That(bestPath.Version.ToString(), Is.EqualTo(scenario.ExpectedVersion));
    }
}