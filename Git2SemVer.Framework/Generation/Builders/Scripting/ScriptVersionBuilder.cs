using NoeticTools.Git2SemVer.Core.Logging;
using NoeticTools.Git2SemVer.Core.Tools.Git;
using NoeticTools.Git2SemVer.Framework.Framework;
using NoeticTools.Git2SemVer.Framework.Framework.BuildHosting;


namespace NoeticTools.Git2SemVer.Framework.Generation.Builders.Scripting;

public sealed class ScriptVersionBuilder : IVersionBuilder
{
    private readonly ILogger _logger;

    public ScriptVersionBuilder(ILogger logger)
    {
        _logger = logger;
    }

    public void Build(IBuildHost host, IGitTool gitTool, IVersionGeneratorInputs inputs, IVersionOutputs outputs)
    {
        if (inputs == null)
        {
            throw new ArgumentException("Build requires non-null inputs.", nameof(inputs));
        }

        if (inputs.RunScript is false)
        {
            _logger.LogDebug("User C# script is disabled. Not run.");
            return;
        }

        if (inputs.RunScript == false)
        {
            _logger.LogDebug("RunScript option is not false. Script not run.");
            return;
        }

        if (!File.Exists(inputs.BuildScriptPath))
        {
            if (inputs.RunScript == null)
            {
                _logger.LogWarning($"RunScript is null and script '{inputs.BuildScriptPath}' not found. Ignoring.");
                return;
            }

            if (inputs.RunScript == true)
            {
                _logger.LogError($"RunScript is true and C# script '{inputs.BuildScriptPath}' not found.");
                return;
            }
        }

        if (!inputs.ValidateScriptInputs(_logger))
        {
            return;
        }

        var context = new VersioningContext(inputs, outputs, host, gitTool, _logger);
        var scriptRunner = new Git2SemVerScriptRunner(new CSharpScriptRunner(_logger), _logger);

        // ReSharper disable once UnusedVariable
        var task = scriptRunner.RunScript(context, inputs.BuildScriptPath);
    }
}