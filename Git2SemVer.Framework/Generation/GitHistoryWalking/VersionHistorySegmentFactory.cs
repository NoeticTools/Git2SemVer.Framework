using NoeticTools.Git2SemVer.Core.Logging;
using NoeticTools.Git2SemVer.Core.Tools.Git;
using System.Diagnostics.CodeAnalysis;


namespace NoeticTools.Git2SemVer.Framework.Generation.GitHistoryWalking;

#pragma warning disable CS1591
[ExcludeFromCodeCoverage]
internal sealed class VersionHistorySegmentFactory(ILogger logger) : IVersionHistorySegmentFactory
{
    private int _nextId = 1;

    public VersionHistorySegment Create(List<Commit> commits)
    {
        return new VersionHistorySegment(_nextId++, commits, logger);
    }

    public VersionHistorySegment Create()
    {
        return new VersionHistorySegment(_nextId++, logger);
    }
}