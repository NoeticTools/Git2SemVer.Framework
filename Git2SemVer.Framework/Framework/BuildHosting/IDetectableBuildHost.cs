namespace NoeticTools.Git2SemVer.Versioning.Framework.BuildHosting;

internal interface IDetectableBuildHost : IBuildHost
{
    bool MatchesHostSignature();
}