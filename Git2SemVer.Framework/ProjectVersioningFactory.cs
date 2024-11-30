using NoeticTools.Git2SemVer.Core.Logging;
using NoeticTools.Git2SemVer.Core.Tools.Git;
using NoeticTools.Git2SemVer.Versioning.Framework.Config;
using NoeticTools.Git2SemVer.Versioning.Generation;
using NoeticTools.Git2SemVer.Versioning.Generation.Builders;
using NoeticTools.Git2SemVer.Versioning.Generation.Builders.Scripting;
using NoeticTools.Git2SemVer.Versioning.Persistence;
using NoeticTools.Git2SemVer.Versioning.Tools.CI;


namespace NoeticTools.Git2SemVer.Versioning;

public sealed class ProjectVersioningFactory
{
    private readonly ILogger _logger;

    public ProjectVersioningFactory(ILogger logger)
    {
        _logger = logger;
    }

    public ProjectVersioning Create(IVersionGeneratorInputs inputs)
    {
        if (inputs == null)
        {
            throw new ArgumentNullException(nameof(inputs), "Inputs is required.");
        }

        var config = Git2SemVerConfiguration.Load();
        var host = new BuildHostFactory(config, _logger).Create(inputs.HostType,
                                                                inputs.BuildNumber,
                                                                inputs.BuildContext,
                                                                inputs.BuildIdFormat);
        var commitsRepo = new CommitsCache();
        var gitProcessCli = new GitProcessCli(_logger) { WorkingDirectory = inputs.WorkingDirectory };
        var gitTool = new GitTool(commitsRepo, gitProcessCli, _logger);
        var gitPathsFinder = new PathsFromLastReleasesFinder(gitTool, _logger);

        var defaultBuilderFactory = new DefaultVersionBuilderFactory(_logger);
        var scriptBuilder = new ScriptVersionBuilder(_logger);
        var generatedOutputsJsonFile = new GeneratedVersionsJsonFile();
        var versionGenerator = new VersionGenerator(inputs,
                                                    host,
                                                    generatedOutputsJsonFile,
                                                    gitTool,
                                                    gitPathsFinder,
                                                    defaultBuilderFactory,
                                                    scriptBuilder,
                                                    _logger);
        var projectVersioning = new ProjectVersioning(inputs, host,
                                                      generatedOutputsJsonFile,
                                                      versionGenerator,
                                                      _logger);
        return projectVersioning;
    }
}