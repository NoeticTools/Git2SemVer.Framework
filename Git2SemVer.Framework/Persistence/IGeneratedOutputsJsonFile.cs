using NoeticTools.Git2SemVer.Versioning.Generation;


namespace NoeticTools.Git2SemVer.Versioning.Persistence;

internal interface IGeneratedOutputsJsonFile
{
    IVersionOutputs Load(string directory);
    void Write(string directory, IVersionOutputs outputs);
}