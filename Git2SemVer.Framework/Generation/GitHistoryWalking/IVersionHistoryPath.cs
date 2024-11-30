using NoeticTools.Git2SemVer.Core.Tools.Git;
using Semver;


namespace NoeticTools.Git2SemVer.Versioning.Generation.GitHistoryWalking;

#pragma warning disable CS1591
public interface IVersionHistoryPath
{
    /// <summary>
    ///     The number of commits to last release (commit depth).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Only used by Git2SemVer, to choose the shortest path if multiple paths found to a release.
    ///     </para>
    /// </remarks>
    int CommitsSinceLastRelease { get; }

    /// <summary>
    ///     The first (oldest) commit in the path.
    ///     This will be either the start of the repository or the commit used for the last release.
    /// </summary>
    Commit FirstCommit { get; }

    /// <summary>
    ///     The youngest commit in the path.
    /// </summary>
    Commit HeadCommit { get; }

    int Id { get; }

    SemVersion? LastReleasedVersion { get; }

    /// <summary>
    ///     The resulting version. Calculated from last release version and Semantic Versioning standard.
    /// </summary>
    SemVersion Version { get; }

    string ToString();
}