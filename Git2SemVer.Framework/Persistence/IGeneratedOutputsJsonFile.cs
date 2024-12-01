﻿using NoeticTools.Git2SemVer.Framework.Generation;


namespace NoeticTools.Git2SemVer.Framework.Persistence;

internal interface IGeneratedOutputsJsonFile
{
    IVersionOutputs Load(string directory);
    void Write(string directory, IVersionOutputs outputs);
}