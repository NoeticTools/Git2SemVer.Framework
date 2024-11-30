﻿using NoeticTools.Git2SemVer.Core.Tools.Git;

namespace NoeticTools.Git2SemVer.Versioning.Generation.GitHistoryWalking;

internal interface IVersionHistorySegmentFactory
{
    VersionHistorySegment Create(List<Commit> commits);
    VersionHistorySegment Create();
}