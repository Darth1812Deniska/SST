using System;
using SSTCommonClasses.Constants;

namespace SqlScriptToolClient.ConstructorStorage.FileStorageSerializeTemplates;

[Serializable]
public class FileStorageModel
{
    public Guid? Uuid { get; set; }
    public string? Name { get; set; }
    public string? DisplayLabel { get; set; }
    public string? Description { get; set; }
    public string? Creator { get; set; }
    public DbType? DbType { get; set; }
    public AccessType? AccessType { get; set; }
    public int? Version { get; set; }
    public Guid? RootModelItem { get; set; }

    public FileStorageModel(Guid uuid, string name, string? displayLabel, string? description, string? creator,
        DbType dbType, AccessType accessType, int version, Guid rootModelItem)
    {
        Uuid = uuid;
        Name = name;
        DisplayLabel = displayLabel;
        Description = description;
        Creator = creator;
        DbType = dbType;
        AccessType = accessType;
        Version = version;
        RootModelItem = rootModelItem;
    }

    public FileStorageModel()
    {
    }
}