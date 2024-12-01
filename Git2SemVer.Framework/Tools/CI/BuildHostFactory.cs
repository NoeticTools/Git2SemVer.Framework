﻿using NoeticTools.Git2SemVer.Core.Logging;
using NoeticTools.Git2SemVer.Framework.Framework.BuildHosting;
using NoeticTools.Git2SemVer.Framework.Framework.Config;


namespace NoeticTools.Git2SemVer.Framework.Tools.CI;

internal sealed class BuildHostFactory
{
    private readonly IConfiguration _config;
    private readonly ILogger _logger;

    public BuildHostFactory(IConfiguration config, ILogger logger)
    {
        _config = config;
        _logger = logger;
    }

    public IBuildHost Create(string hostType, string buildNumber, string buildContext, string inputsBuildIdFormat)
    {
        var host = new BuildHost(new BuildHostFinder(_config, _logger).Find(hostType), _logger);

        if (!string.IsNullOrWhiteSpace(buildNumber))
        {
            host.BuildNumber = buildNumber;
        }

        if (!string.IsNullOrWhiteSpace(buildContext))
        {
            host.BuildContext = buildContext;
        }

        if (!string.IsNullOrWhiteSpace(inputsBuildIdFormat))
        {
            host.BuildIdFormat = inputsBuildIdFormat;
        }

        _logger.LogInfo($"Using '{host.Name}' host. Build ID: {string.Join(".", host.BuildId)}");

        return host;
    }
}