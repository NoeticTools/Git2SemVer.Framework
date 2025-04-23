using NoeticTools.Git2SemVer.Core.Tools.Git;


namespace NoeticTools.Git2SemVer.Framework.Generation.GitHistoryWalking;

internal interface IVersionHistorySegmentsBuilder
{
    /// <summary>
    /// Get git history path segments from prior release tags to the given commit.
    /// </summary>
    IReadOnlyList<VersionHistorySegment> BuildTo(Commit commit, bool forcePriorRelease = false);
}