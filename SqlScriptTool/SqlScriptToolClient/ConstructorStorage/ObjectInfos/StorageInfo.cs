using SSTCommonClasses.Constants;

namespace SqlScriptToolClient.ConstructorStorage.ObjectInfos;

public class StorageInfo
{
    public AccessType AccessType { get; set; }
    public ConstructorObjectTypes ConstructorObjectTypes { get; set; }
    public string Path { get; set; }

    public StorageInfo(AccessType accessType, ConstructorObjectTypes constructorObjectTypes, string path)
    {
        AccessType = accessType;
        ConstructorObjectTypes = constructorObjectTypes;
        Path = path;
    }
}