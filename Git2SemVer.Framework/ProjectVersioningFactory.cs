using System.Diagnostics.CodeAnalysis;
using NoeticTools.Git2SemVer.Core.Logging;
using NoeticTools.Git2SemVer.Core.Tools.Git;
using NoeticTools.Git2SemVer.Framework.Framework.Config;
using NoeticTools.Git2SemVer.Framework.Generation;
using NoeticTools.Git2SemVer.Framework.Generation.Builders;
using NoeticTools.Git2SemVer.Framework.Generation.Builders.Scripting;
using NoeticTools.Git2SemVer.Framework.Persistence;
using NoeticTools.Git2SemVer.Framework.Tools.CI;


namespace NoeticTools.Git2SemVer.Framework;

[ExcludeFromCodeCoverage]
public sealed class ProjectVersioningFactory
{
    private readonly ILogger _logger;

    public ProjectVersioningFactory(ILogger logger)
    {
        _logger = logger;
    }

    public ProjectVersioning Create(IVersionGeneratorInputs inputs, IOutputsJsonIO? outputsJsonIO = null, IConfiguration? config = null)
    {
        if (inputs == null)
        {
            throw new ArgumentNullException(nameof(inputs), "Inputs is required.");
        }

        outputsJsonIO ??= new OutputsJsonFileIO();
        config ??= Git2SemVerConfiguration.Load();

        var host = new BuildHostFactory(config, _logger).Create(inputs.HostType,
                                                                inputs.BuildNumber,
                                                                inputs.BuildContext,
                                                                inputs.BuildIdFormat);
        var gitTool = new GitTool(_logger) { WorkingDirectory = inputs.WorkingDirectory };
        var gitPathsFinder = new PathsFromLastReleasesFinder(gitTool, _logger);

        var defaultBuilderFactory = new DefaultVersionBuilderFactory(_logger);
        var scriptBuilder = new ScriptVersionBuilder(_logger);
        var versionGenerator = new VersionGenerator(inputs,
                                                    host,
                                                    outputsJsonIO,
                                                    gitTool,
                                                    gitPathsFinder,
                                                    defaultBuilderFactory,
                                                    scriptBuilder,
                                                    _logger);
        var projectVersioning = new ProjectVersioning(inputs, host,
                                                      outputsJsonIO,
                                                      versionGenerator,
                                                      _logger);
        return projectVersioning;
    }
}