using NoeticTools.Git2SemVer.Core.Logging;
using NoeticTools.Git2SemVer.Versioning.Framework.BuildHosting;
using NoeticTools.Git2SemVer.Versioning.Generation;
using NoeticTools.Git2SemVer.Versioning.Persistence;


#pragma warning disable CA1859

namespace NoeticTools.Git2SemVer.Versioning;

public sealed class ProjectVersioning
{
    private readonly IVersionGeneratorInputs _inputs;
    private readonly IBuildHost _host;
    private readonly IGeneratedOutputsJsonFile _outputsCacheJsonFile;
    private readonly IVersionGenerator _versionGenerator;
    private readonly ILogger _logger;

    internal ProjectVersioning(
        IVersionGeneratorInputs inputs,
        IBuildHost host,
        IGeneratedOutputsJsonFile outputsCacheJsonFile,
        IVersionGenerator versionGenerator,
        ILogger logger)
    {
        _inputs = inputs;
        _host = host;
        _outputsCacheJsonFile = outputsCacheJsonFile;
        _versionGenerator = versionGenerator;
        _logger = logger;
    }

    public IVersionOutputs Run()
    {
        try
        {
            var handlers = new Dictionary<VersioningMode, Func<IVersionOutputs>>
            {
                { VersioningMode.SolutionVersioningProject, PerformSolutionVersioningProjectVersioning },
                { VersioningMode.SolutionClientProject, PerformSolutionClientVersioning },
                { VersioningMode.StandAloneProject, PerformStandAloneProjectVersioning }
            };

            var outputs = handlers[_inputs.VersioningMode]();
            UpdateHostBuildLabel(outputs);
            return outputs;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception);
            throw;
        }
    }

    private string GetClientLastBuildNumber()
    {
        var localCache = _outputsCacheJsonFile.Load(_inputs.IntermediateOutputDirectory);
        if (localCache.IsValid)
        {
            return localCache.BuildNumber;
        }

        var shared = _outputsCacheJsonFile.Load(_inputs.SolutionSharedDirectory);
        return !shared.IsValid ? _host.BuildNumber : shared.BuildNumber;
    }

    private IVersionOutputs PerformSolutionClientVersioning()
    {
        _logger.LogTrace("Solution versioning client project.");

        var lastBuildNumber = GetClientLastBuildNumber();
        if (lastBuildNumber == _host.BuildNumber)
        {
            return _versionGenerator.Run();
        }

        var output = _outputsCacheJsonFile.Load(_inputs.SolutionSharedDirectory);
        _outputsCacheJsonFile.Write(_inputs.IntermediateOutputDirectory, output);
        return output;
    }

    private IVersionOutputs PerformSolutionVersioningProjectVersioning()
    {
        _logger.LogTrace("Solution versioning project.");
        var output = _outputsCacheJsonFile.Load(_inputs.SolutionSharedDirectory);
        return !output.IsValid ? _versionGenerator.Run() : output;
    }

    private IVersionOutputs PerformStandAloneProjectVersioning()
    {
        _logger.LogTrace("Stand alone project versioning.");
        return _versionGenerator.Run();
    }

    private void UpdateHostBuildLabel(IVersionOutputs output)
    {
        if (_inputs.UpdateHostBuildLabel && output.BuildSystemVersion != null)
        {
            _host.SetBuildLabel(output.BuildSystemVersion.ToString());
        }
    }
}