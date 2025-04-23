using System.Diagnostics;
using NoeticTools.Git2SemVer.Core.Logging;
using NoeticTools.Git2SemVer.Core.Tools.Git;
using NoeticTools.Git2SemVer.Framework.Framework.BuildHosting;
using NoeticTools.Git2SemVer.Framework.Generation.Builders;
using NoeticTools.Git2SemVer.Framework.Generation.GitHistoryWalking;
using NoeticTools.Git2SemVer.Framework.Persistence;


namespace NoeticTools.Git2SemVer.Framework.Generation;

internal sealed class VersionGenerator(
    IVersionGeneratorInputs inputs,
    IBuildHost host,
    IOutputsJsonIO generatedOutputsJsonFile,
    IGitTool gitTool,
    IGitHistoryPathsFinder gitPathsFinder,
    IDefaultVersionBuilderFactory defaultVersionBuilderFactory,
    IVersionBuilder scriptBuilder,
    ILogger logger)
    : IVersionGenerator
{
    public IVersionOutputs Run()
    {
        var stopwatch = Stopwatch.StartNew();

        host.BumpBuildNumber();

        var historyPaths = gitPathsFinder.GetPathsToHeadFromPriorRelease();

        var outputs = new VersionOutputs(new GitOutputs(gitTool, historyPaths));
        RunBuilders(outputs, historyPaths);
        SaveGeneratedVersions(outputs);

        stopwatch.Stop();

        logger.LogTrace("");
        logger.LogInfo($"Informational version: {outputs.InformationalVersion}");

        logger.LogDebug($"Version generation completed. Took {stopwatch.Elapsed.TotalSeconds:F1} seconds.");

        host.ReportBuildStatistic("Git2SemVerRunTime_sec", stopwatch.Elapsed.TotalSeconds);

        return outputs;
    }

    private void RunBuilders(VersionOutputs outputs, IHistoryPaths historyPaths)
    {
        logger.LogDebug("");
        logger.LogDebug("Running version builders.");
        using (logger.EnterLogScope())
        {
            var stopwatch = Stopwatch.StartNew();

            defaultVersionBuilderFactory.Create(historyPaths).Build(host, gitTool, inputs, outputs);
            if (logger.Level >= LoggingLevel.Trace)
            {
                logger.LogTrace(outputs.GetReport());
            }

            scriptBuilder.Build(host, gitTool, inputs, outputs);
            if (logger.Level >= LoggingLevel.Debug)
            {
                logger.LogDebug(outputs.GetReport());
            }

            stopwatch.Stop();
            logger.LogDebug("");
            logger.LogDebug($"Version building completed ({stopwatch.Elapsed.TotalSeconds:F1} sec).\n");
        }
    }

    private void SaveGeneratedVersions(VersionOutputs outputs)
    {
        generatedOutputsJsonFile.Write(inputs.IntermediateOutputDirectory, outputs);
        if (inputs.VersioningMode != VersioningMode.StandAloneProject)
        {
            generatedOutputsJsonFile.Write(inputs.SolutionSharedDirectory, outputs);
        }
    }
}