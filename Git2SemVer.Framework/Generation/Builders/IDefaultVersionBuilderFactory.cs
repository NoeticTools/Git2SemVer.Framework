using NoeticTools.Git2SemVer.Versioning.Generation.GitHistoryWalking;


namespace NoeticTools.Git2SemVer.Versioning.Generation.Builders;

internal interface IDefaultVersionBuilderFactory
{
    IVersionBuilder Create(HistoryPaths historyPaths);
}