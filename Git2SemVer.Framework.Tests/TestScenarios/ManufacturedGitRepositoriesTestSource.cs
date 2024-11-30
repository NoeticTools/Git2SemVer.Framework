﻿using System.Collections;
using NoeticTools.Git2SemVer.Core.ConventionCommits;
using NoeticTools.Git2SemVer.Core.Tools.Git;


namespace NoeticTools.Git2SemVer.Framework.Tests.TestScenarios;

public sealed class ManufacturedGitRepositoriesTestSource : IEnumerable
{
    public IEnumerator GetEnumerator()
    {
        yield return new object[] { "Scenario 01", BuildScenario01() };
        yield return new object[] { "Scenario 02", BuildScenario02() };
        yield return new object[] { "Scenario 03", BuildScenario03() };
        yield return new object[] { "Scenario 04", BuildScenario04() };
        yield return new object[] { "Scenario 06", BuildScenario06() };
    }

    private static GitTestRepository BuildScenario01()
    {
        return new GitTestRepository("""
                                     Scenario 01:
                                       - Head is master branch (1)
                                       - No commit bump messages
                                       - No releases on master
                                       - 2 releases on a merged branch
                                       
                                       | head
                                       |
                                       |\____   branch 3
                                       |     \
                                       |      | v1.2.4
                                       |      | v1.2.3
                                       | ____/
                                       |/
                                       |
                                       |\____   branch 2
                                       |     \
                                       |      |
                                       | ____/
                                       |/
                                       |
                                       | first commit
                                     """,
                                     [
                                         new Commit("1.001.0000", [], "First commit in repo", "", "", new CommitMessageMetadata()),
                                         new Commit("1.002.0000", ["1.001.0000"], "", "", "tag: A tag", new CommitMessageMetadata()),
                                         new Commit("1.003.0000", ["1.002.0000"], "", "", "tag: A6.7.8", new CommitMessageMetadata()),
                                         new Commit("1.004.0000", ["1.003.0000", "2.003.0000"], "Merge", "", "", new CommitMessageMetadata()),
                                         new Commit("1.005.0000", ["1.004.0000"], "", "", "", new CommitMessageMetadata()),
                                         new Commit("1.006.0000", ["1.005.0000"], "", "", "", new CommitMessageMetadata()),
                                         new Commit("1.007.0000", ["1.006.0000"], "", "", "", new CommitMessageMetadata()),
                                         new Commit("1.008.0000", ["1.007.0000", "3.005.0000"], "Merge", "", "", new CommitMessageMetadata()),
                                         new Commit("1.009.0000", ["1.008.0000"], "", "", "", new CommitMessageMetadata()),
                                         new Commit("1.010.0000", ["1.009.0000"], "Head commit", "", "", new CommitMessageMetadata()),

                                         new Commit("2.001.0000", ["1.001.0000"], "Branch commit", "", "", new CommitMessageMetadata()),
                                         new Commit("2.002.0000", ["2.001.0000"], "", "", "", new CommitMessageMetadata()),
                                         new Commit("2.003.0000", ["2.002.0000"], "", "", "", new CommitMessageMetadata()),

                                         new Commit("3.001.0000", ["1.007.0000"], "Branch commit", "", "", new CommitMessageMetadata()),
                                         new Commit("3.002.0000", ["3.001.0000"], "", "", "tag: v1.2.3", new CommitMessageMetadata()),
                                         new Commit("3.003.0000", ["3.002.0000"], "", "", "tag: v1.2.4", new CommitMessageMetadata()),
                                         new Commit("3.004.0000", ["3.003.0000"], "", "", "", new CommitMessageMetadata()),
                                         new Commit("3.005.0000", ["3.004.0000"], "", "", "", new CommitMessageMetadata())
                                     ],
                                     "1.010.0000",
                                     3,
                                     "1.2.5");
    }

    private static GitTestRepository BuildScenario02()
    {
        return new GitTestRepository("""
                                     Scenario 02:
                                       - Release on merge commit with release on branch being merged
                                       - Head is master branch (1)
                                       - No commit bump messages
                                       - Release 1.2.2 on master on branch merge commit
                                       - Release 1.2.4 on branch that has been merged to master

                                     1.010  | head (1)
                                     .      |
                                     .      |\____   branch 2 (merge) - v1.2.2
                                     .      |     \
                                     .      |      | v1.2.4
                                     .      | ____/
                                     .      |/
                                     .      |
                                     1.001  | first commit
                                     """,
                                     [
                                         new Commit("1.001.0000", [], "First commit in repo", "", "", new CommitMessageMetadata()),
                                         new Commit("1.007.0000", ["1.001.0000"], "", "", "Branched from", new CommitMessageMetadata()),
                                         new Commit("1.008.0000", ["1.007.0000", "2.005.0000"], "Merge commit", "", "tag: v1.2.2",
                                                    new CommitMessageMetadata()),
                                         new Commit("1.009.0000", ["1.008.0000"], "", "", "", new CommitMessageMetadata()),
                                         new Commit("1.010.0000", ["1.009.0000"], "Head commit", "", "", new CommitMessageMetadata()),

                                         new Commit("2.001.0000", ["1.007.0000"], "Branch commit", "", "", new CommitMessageMetadata()),
                                         new Commit("2.003.0000", ["2.001.0000"], "", "", "tag: v1.2.4", new CommitMessageMetadata()),
                                         new Commit("2.005.0000", ["2.003.0000"], "", "", "", new CommitMessageMetadata())
                                     ],
                                     "1.010.0000",
                                     1,
                                     "1.2.3");
    }

    private static GitTestRepository BuildScenario03()
    {
        return new GitTestRepository("""
                                     Scenario 03:
                                       - Single branch, takes first release
                                       - Head is master branch (1)
                                       - No commit bump messages
                                       - Release 1.5.9 on master
                                       - Release 2.2.2 on prior commit on master

                                     1.006  | head
                                     .      | v1.5.9
                                     .      |
                                     .      | v2.2.2
                                     .      |
                                     1.001  | first commit
                                     """,
                                     [
                                         new Commit("1.001", [], "First commit in repo", "", "", new CommitMessageMetadata()),
                                         new Commit("1.002", ["1.001"], "", "", "", new CommitMessageMetadata()),
                                         new Commit("1.003", ["1.002"], "", "", "tag: v2.2.2", new CommitMessageMetadata()),
                                         new Commit("1.004", ["1.003"], "", "", "", new CommitMessageMetadata()),
                                         new Commit("1.005", ["1.004"], "", "", "tag: V1.5.9", new CommitMessageMetadata()),
                                         new Commit("1.006", ["1.005"], "", "", "Head commit", new CommitMessageMetadata())
                                     ],
                                     "1.006",
                                     1,
                                     "1.5.10");
    }

    private static GitTestRepository BuildScenario04()
    {
        return new GitTestRepository("""
                                     Scenario 04:
                                       - Single commit repository
                                       - No releases

                                     1.001  | head
                                     """,
                                     [
                                         new Commit("1.001.0000", [], "First commit in repo", "", "", new CommitMessageMetadata())
                                     ],
                                     "1.001.0000",
                                     1,
                                     "0.1.0");
    }

    private static GitTestRepository BuildScenario06()
    {
        return new GitTestRepository("""
                                     Scenario 06:
                                       - Multiple parallel branches
                                       - Releases in every branch

                                     1.006  | head
                                     .      |
                                     1.005  |\____________________________   branch 3 (merge)
                                     1.004  |\_______   branch 2 (merge)   \
                                     .      |         \                     | v5.6.99  
                                     .      | v5.7.0   | v5.7.1             |
                                     .      | ________/____________________/
                                     .      |/
                                     .      |
                                     1.001  | first commit
                                     """,
                                     [
                                         new Commit("1.001.0000", [], "First commit in repo", "", "", new CommitMessageMetadata()),
                                         new Commit("1.002.0000", ["1.001.0000"], "Branch from", "", "", new CommitMessageMetadata()),
                                         new Commit("1.003.0000", ["1.002.0000"], "", "", "tag: v5.7.0", new CommitMessageMetadata()),
                                         new Commit("1.004.0000", ["1.003.0000", "2.003.0000"], "Merge", "", "", new CommitMessageMetadata()),
                                         new Commit("1.005.0000", ["1.004.0000", "3.003.0000"], "Merge", "", "", new CommitMessageMetadata()),
                                         new Commit("1.006.0000", ["1.005.0000"], "Head commit", "", "", new CommitMessageMetadata()),

                                         new Commit("2.001.0000", ["1.002.0000"], "Branch", "", "", new CommitMessageMetadata()),
                                         new Commit("2.002.0000", ["2.001.0000"], "", "", "tag: v5.7.1", new CommitMessageMetadata()),
                                         new Commit("2.003.0000", ["2.002.0000"], "", "", "", new CommitMessageMetadata()),

                                         new Commit("3.001.0000", ["1.002.0000"], "Branch", "", "", new CommitMessageMetadata()),
                                         new Commit("3.002.0000", ["3.001.0000"], "", "", "tag: v5.6.99", new CommitMessageMetadata()),
                                         new Commit("3.003.0000", ["3.002.0000"], "", "", "", new CommitMessageMetadata())
                                     ],
                                     "1.006.0000",
                                     3,
                                     "5.7.2");
    }
}