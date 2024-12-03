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
            throw new ArgumentException("Script version builder requires non-null inputs.", nameof(inputs));
        }

        if (inputs.RunScript == false)
        {
            _logger.LogDebug("Skipped user C# script as option not enabled.");
            return;
        }

        if (!File.Exists(inputs.BuildScriptPath))
        {
            if (inputs.RunScript == null)
            {
                _logger.LogDebug($"User C# script '{inputs.BuildScriptPath}' was not found. Ignoring.");
                return;
            }

            if (inputs.RunScript == true)
            {
                _logger.LogError($"C# script '{inputs.BuildScriptPath}' was not found.");
                return;
            }
        }

        if (!inputs.ValidateScriptInputs(_logger))
        {
            return;
        }

        _logger.LogDebug("Running user C# script version builder.\n");
        using (_logger.EnterLogScope())
        {
            var context = new VersioningContext(inputs, outputs, host, gitTool, _logger);
            var scriptRunner = new Git2SemVerScriptRunner(new CSharpScriptRunner(_logger), _logger);

            // ReSharper disable once UnusedVariable
            var task = scriptRunner.RunScript(context, inputs.BuildScriptPath);
        }
    }
}