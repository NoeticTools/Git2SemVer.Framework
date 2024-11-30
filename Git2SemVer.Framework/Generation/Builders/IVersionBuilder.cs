using NoeticTools.Git2SemVer.Core.Tools.Git;
using NoeticTools.Git2SemVer.Versioning.Framework.BuildHosting;
using NoeticTools.Git2SemVer.Versioning.Generation;


namespace NoeticTools.Git2SemVer.Versioning.Generation.Builders;

/// <summary>
///     An output builder that sets or updates any of the task's MSBuild output properties.
/// </summary>
public interface IVersionBuilder
{
    void Build(IBuildHost host, IGitTool gitTool, IVersionGeneratorInputs inputs, IVersionOutputs outputs);
}