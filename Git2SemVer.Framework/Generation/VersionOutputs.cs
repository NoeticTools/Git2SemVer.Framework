﻿using System.Text.Json.Serialization;
using NoeticTools.Git2SemVer.Framework.Framework.Semver;
using Semver;


namespace NoeticTools.Git2SemVer.Framework.Generation;

public sealed class VersionOutputs : IVersionOutputs
{
    [JsonConstructor]
    public VersionOutputs() : this(new GitOutputs())
    {
    }

    public VersionOutputs(GitOutputs gitOutputs)
    {
        Git = gitOutputs;
    }

    public Version? AssemblyVersion { get; set; }

    public string BuildContext { get; set; } = "";

    public string BuildNumber { get; set; } = "";

    [JsonConverter(typeof(SemVersionJsonConverter))]
    public SemVersion? BuildSystemVersion { get; set; }

    public Version? FileVersion { get; set; }

    public IGitOutputs Git { get; }

    [JsonConverter(typeof(SemVersionJsonConverter))]
    public SemVersion? InformationalVersion { get; set; }

    public bool IsInInitialDevelopment { get; set; }

    [JsonIgnore]
    public bool IsValid => BuildNumber.Length > 0;

    public string Output1 { get; set; } = "";

    public string Output2 { get; set; } = "";

    [JsonConverter(typeof(SemVersionJsonConverter))]
    public SemVersion? PackageVersion { get; set; }

    public string PrereleaseLabel { get; set; } = "";

    [JsonConverter(typeof(SemVersionJsonConverter))]
    public SemVersion? Version { get; set; }

    public void SetAllVersionPropertiesFrom(SemVersion informationalVersion,
                                            string buildNumber,
                                            string buildContext)
    {
        SetAllVersionPropertiesFrom(informationalVersion);
        BuildNumber = buildNumber;
        BuildContext = buildContext;
    }

    public void SetAllVersionPropertiesFrom(SemVersion informationalVersion)
    {
        var version = informationalVersion.WithoutMetadata();
        var versionPrefix = informationalVersion.WithoutMetadata()
                                                .WithoutPrerelease();
        InformationalVersion = informationalVersion;
        Version = version;
        AssemblyVersion = new Version(versionPrefix.ToString());
        FileVersion = new Version(versionPrefix.ToString());
        PackageVersion = version;
        BuildSystemVersion = version;
        PrereleaseLabel = informationalVersion.IsRelease
            ? ""
            : informationalVersion.PrereleaseIdentifiers[0];
        IsInInitialDevelopment = informationalVersion.Major == 0;
    }
}