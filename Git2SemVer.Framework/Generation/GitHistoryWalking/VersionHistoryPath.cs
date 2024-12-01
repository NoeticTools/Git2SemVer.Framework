using NoeticTools.Git2SemVer.Core.ConventionCommits;
using NoeticTools.Git2SemVer.Core.Tools.Git;
using Semver;


namespace NoeticTools.Git2SemVer.Framework.Generation.GitHistoryWalking;

#pragma warning disable CS1591

internal sealed class VersionHistoryPath : IVersionHistoryPath
{
    private readonly List<VersionHistorySegment> _segments = [];
    private ApiChanges _bumps = new();
    private int? _commitsCount;

    public VersionHistoryPath(params VersionHistorySegment[] segments)
    {
        _segments.AddRange(segments);
        LastReleasedVersion = segments.First().TaggedReleasedVersion;
        Version = GetNextReleaseVersion();
    }

    public int CommitsSinceLastRelease
    {
        get
        {
            _commitsCount ??= _segments.Sum(x => x.Commits.Count);
            return _commitsCount.Value;
        }
    }

    public Commit FirstCommit => _segments.First().OldestCommit;

    public Commit HeadCommit => _segments.Last().YoungestCommit;

    public int Id { get; internal set; }

    public SemVersion? LastReleasedVersion { get; }

    public SemVersion Version { get; }

    public override string ToString()
    {
        var commitsCount = $"({CommitsSinceLastRelease})";
        var segmentIdsString = string.Join("-", _segments.Select(x => x.Id));
        return
            $"Path {Id,-3} {segmentIdsString,-20} {commitsCount,5}   {_bumps}   {LastReleasedVersion?.ToString() ?? " none"} -> {GetNextReleaseVersion()}";
    }

    public VersionHistoryPath With(VersionHistorySegment segment)
    {
        var segmentIds = new List<VersionHistorySegment>(_segments) { segment };
        return new VersionHistoryPath(segmentIds.ToArray());
    }

    private ApiChanges AggregateBumps()
    {
        var bumps = new ApiChanges();
        foreach (var segmentBumps in _segments.Select(segment => segment.ApiChangeFlags))
        {
            bumps.BreakingChange |= segmentBumps.BreakingChange;
            bumps.FunctionalityChange |= segmentBumps.FunctionalityChange;
            bumps.Fix |= segmentBumps.Fix;
        }

        return bumps;
    }

    private SemVersion GetNextReleaseVersion()
    {
        _bumps = AggregateBumps();

        if (HeadCommit.HasReleaseTag)
        {
            return HeadCommit.ReleasedVersion!;
        }

        var startingVersion = LastReleasedVersion ?? new SemVersion(0, 1, 0);

        if (_bumps.BreakingChange)
        {
            return new SemVersion(startingVersion.Major + 1, 0, 0);
        }

        if (_bumps.FunctionalityChange)
        {
            return new SemVersion(startingVersion.Major, startingVersion.Minor + 1, 0);
        }

        return LastReleasedVersion == null ? startingVersion : new SemVersion(startingVersion.Major, startingVersion.Minor, startingVersion.Patch + 1);
    }
}