using NoeticTools.Git2SemVer.Framework.Generation.GitHistoryWalking;


namespace NoeticTools.Git2SemVer.Framework.Generation.Builders;

internal interface IDefaultVersionBuilderFactory
{
    IVersionBuilder Create(HistoryPaths historyPaths);
}