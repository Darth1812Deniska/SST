using SSTCommonClasses.Constants;

namespace SqlScriptToolClient.ConstructorStorage.ObjectInfos;

public class StorageInfo
{
    public AccessType AccessType { get; set; }
    public ConstructorType ConstructorObjectTypes { get; set; }
    public string Path { get; set; }

    public StorageInfo(AccessType accessType, ConstructorType constructorObjectTypes, string path)
    {
        AccessType = accessType;
        ConstructorObjectTypes = constructorObjectTypes;
        Path = path;
    }
}