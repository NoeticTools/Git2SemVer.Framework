﻿using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using NoeticTools.Git2SemVer.Core.Git2SemVer;
using NoeticTools.Git2SemVer.Framework.Generation;


namespace NoeticTools.Git2SemVer.Framework.Persistence;

public sealed class GeneratedVersionsJsonFile : IGeneratedOutputsJsonFile
{
    private static readonly JsonSerializerOptions SerialiseOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin),
        IncludeFields = false
    };

    public IVersionOutputs Load(string directory)
    {
        var propertiesFilePath = GetFilePath(directory);
        if (!File.Exists(propertiesFilePath))
        {
            return new VersionOutputs();
        }

        var json = File.ReadAllText(propertiesFilePath);
        return JsonSerializer.Deserialize<VersioningInfo>(json)!.Git2SemVerVersionInfo!;
    }

    public static string ToJson(IVersionOutputs outputs)
    {
        var versionInfo = new VersioningInfo { Git2SemVerVersionInfo = (VersionOutputs)outputs };
        return JsonSerializer.Serialize(versionInfo, SerialiseOptions);
    }

    public void Write(string directory, IVersionOutputs outputs)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = ToJson(outputs);
        var existingJson = LoadJson(directory);
        if (json.Equals(existingJson, StringComparison.Ordinal))
        {
            return;
        }

        File.WriteAllText(GetFilePath(directory), json);
    }

    private static string GetFilePath(string directory)
    {
        return Path.Combine(directory, Git2SemVerConstants.SharedVersionJsonPropertiesFilename);
    }

    private static string LoadJson(string directory)
    {
        var propertiesFilePath = GetFilePath(directory);
        return !File.Exists(propertiesFilePath) ? "" : File.ReadAllText(propertiesFilePath);
    }

    private sealed class VersioningInfo
    {
        [JsonPropertyOrder(2)]
        public VersionOutputs? Git2SemVerVersionInfo { get; set; }

        /// <summary>
        ///     This version info's schema version.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <strong>Rev 2</strong>
        ///         <list type="bullet">
        ///             <item>Generated JSON - CommitId `Id` property renamed to `Sha`</item>
        ///             <item>Generated JSON - CommitChangeTypeId.None added, type numbers bumped.</item>
        ///         </list>
        ///     </para>
        /// </remarks>
        [JsonPropertyOrder(1)]
        public int Rev { get; set; } = 2;
    }
}