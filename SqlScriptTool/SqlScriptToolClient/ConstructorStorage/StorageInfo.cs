using SSTCommonClasses.Constants;
using SSTCommonClasses.Models;

namespace SqlScriptToolClient.ConstructorStorage;

public class StorageInfo
{
    public ModelAccessType ModelAccessType { get; set; }
    public ConstructorObjectTypes ConstructorObjectTypes { get; set; }
    public string Path { get; set; }

    public StorageInfo(ModelAccessType modelAccessType, ConstructorObjectTypes constructorObjectTypes, string path)
    {
        ModelAccessType = modelAccessType;
        ConstructorObjectTypes = constructorObjectTypes;
        Path = path;
    }
}