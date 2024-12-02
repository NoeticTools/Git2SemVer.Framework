using NoeticTools.Git2SemVer.Core.Logging;
using NoeticTools.Git2SemVer.Core.Tools.Git;
using NoeticTools.Git2SemVer.Framework.Framework.Config;
using NoeticTools.Git2SemVer.Framework.Generation;
using NoeticTools.Git2SemVer.Framework.Generation.Builders;
using NoeticTools.Git2SemVer.Framework.Generation.Builders.Scripting;
using NoeticTools.Git2SemVer.Framework.Persistence;
using NoeticTools.Git2SemVer.Framework.Tools.CI;


namespace NoeticTools.Git2SemVer.Framework;

public sealed class ProjectVersioningFactory
{
    private readonly IGeneratedOutputsJsonFile _outputsJsonFile;
    private readonly ILogger _logger;

    public ProjectVersioningFactory(IGeneratedOutputsJsonFile outputsJsonFile, IConfiguration config, ILogger logger)
    {
        _outputsJsonFile = outputsJsonFile;
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
        var versionGenerator = new VersionGenerator(inputs,
                                                    host,
                                                    _outputsJsonFile,
                                                    gitTool,
                                                    gitPathsFinder,
                                                    defaultBuilderFactory,
                                                    scriptBuilder,
                                                    _logger);
        var projectVersioning = new ProjectVersioning(inputs, host,
                                                      _outputsJsonFile,
                                                      versionGenerator,
                                                      _logger);
        return projectVersioning;
    }
}