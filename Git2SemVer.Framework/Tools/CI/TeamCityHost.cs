using System.Globalization;
using JetBrains.TeamCity.ServiceMessages.Write.Special;
using NoeticTools.Git2SemVer.Core.Logging;
using NoeticTools.Git2SemVer.Framework.Framework.BuildHosting;


namespace NoeticTools.Git2SemVer.Framework.Tools.CI;

internal class TeamCityHost : BuildHostBase, IDetectableBuildHost
{
    private const string BuildNumberEnvVarName = "BUILD_NUMBER";
    private const string TeamCityVersionEnvVarName = "TEAMCITY_VERSION";
    private readonly ILogger _logger;
    private readonly string _teamCityVersion;

    public TeamCityHost(ILogger logger) : base(logger)
    {
        _logger = logger;
        Name = "TeamCity";
        _teamCityVersion = Environment.GetEnvironmentVariable(TeamCityVersionEnvVarName) ?? "";
        if (_teamCityVersion.Length > 0)
        {
            BuildNumber = GetBuildNumber();
        }

        BuildContext = "0";
        DefaultBuildNumberFunc = () => [BuildNumber];
    }

    public HostTypeIds HostTypeId => HostTypeIds.TeamCity;

    public bool MatchesHostSignature()
    {
        var result = !string.IsNullOrWhiteSpace(_teamCityVersion) &&
                     !string.IsNullOrWhiteSpace(BuildNumber);
        if (result)
        {
            _logger.LogInfo("Detected build running on TeamCity.");
        }

        return result;
    }

    public override void ReportBuildStatistic(string key, int value)
    {
        _logger.LogInfo($"Build statistic {key} = {value}");
        using var writer = new TeamCityServiceMessages().CreateWriter(_logger.LogInfo);
        writer.WriteBuildStatistics(key, value.ToString(CultureInfo.InvariantCulture));
    }

    public override void ReportBuildStatistic(string key, double value)
    {
        _logger.LogInfo($"Build statistic {key} = {value:G13}");
        using var writer = new TeamCityServiceMessages().CreateWriter(_logger.LogInfo);
        writer.WriteBuildStatistics(key, $"{value:G13}");
    }

    public override void SetBuildLabel(string label)
    {
        _logger.LogInfo($"Setting TeamCity Build label to '{label}'.");
        using var writer = new TeamCityServiceMessages().CreateWriter(_logger.LogInfo);
        writer.WriteBuildNumber(label);
    }

    private static string GetBuildNumber()
    {
        var buildNumberVariable = Environment.GetEnvironmentVariable(BuildNumberEnvVarName);
        return int.TryParse(buildNumberVariable!, out var buildNumber) ? buildNumber.ToString(CultureInfo.InvariantCulture) : "";
    }
}