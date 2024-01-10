using System;
using SSTCommonClasses.Constants;
using SSTCommonClasses.Models;

namespace SqlScriptToolClient.ConstructorStorage.ObjectInfos;

public class ModelInfo
{
    public Guid ModelUuid { get; set; }
    public string ModelName { get; set; }
    public AccessType ModelAccessType { get; set; }
    public string ModelPath { get; set; }
    public string ModelFileName { get; set; }
    public string ModelItemsFileName { get; set; }

    public ModelInfo()
    {
    }

    public ModelInfo(Guid modelGuid, string modelName, AccessType modelAccessType, string modelPath,
        string modelFileName, string modelItemsFileName)
    {
        ModelUuid = modelGuid;
        ModelName = modelName;
        ModelAccessType = modelAccessType;
        ModelPath = modelPath;
        ModelFileName = modelFileName;
        ModelItemsFileName = modelItemsFileName;
    }

    public bool Equals(ModelInfo? other)
    {
        if (other == null)
            return false;
        return ModelUuid == other.ModelUuid;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }

        return obj is ModelInfo model && Equals(model);
    }
}