using System.Globalization;
using NoeticTools.Git2SemVer.Core.Logging;
using NoeticTools.Git2SemVer.Versioning.Framework.BuildHosting;
using NoeticTools.Git2SemVer.Versioning.Framework.Config;
using NoeticTools.Git2SemVer.Versioning.Framework.Semver;


namespace NoeticTools.Git2SemVer.Versioning.Tools.CI;

internal class UncontrolledHost : BuildHostBase, IDetectableBuildHost
{
    private readonly IConfiguration _config;
    private readonly ILogger _logger;

    public UncontrolledHost(IConfiguration config, ILogger logger) : base(logger)
    {
        _config = config;
        _logger = logger;
        Name = "Uncontrolled";
        BuildContext = Environment.MachineName.ToNormalisedSemVerIdentifier();
        BuildNumber = _config.BuildNumber.ToString(CultureInfo.InvariantCulture);
        DefaultBuildNumberFunc = () => [BuildContext, BuildNumber];
    }

    public HostTypeIds HostTypeId => HostTypeIds.Uncontrolled;

    public override string BumpBuildNumber()
    {
        _config.BuildNumber++;
        _config.Save();
        _logger.LogDebug("Bumping build number to {0}.", _config.BuildNumber);
        BuildNumber = _config.BuildNumber.ToString(CultureInfo.InvariantCulture);
        return BuildNumber;
    }

    public bool MatchesHostSignature()
    {
        return true;
    }
}