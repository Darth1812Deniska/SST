using System;
using System.Collections.Generic;
using SSTCommonClasses.Constants;

namespace SqlScriptToolAvalonia.ConstructorStorage.FileStorageSerializeTemplates;

[Serializable]
public class FileStorageSqlTemplate
{
    public Guid? Uuid { get; set; }
    public string? Name { get; set; }
    public string? DisplayLabel { get; set; }
    public string? Description { get; set; }
    public DbType? DbType { get; set; }
    public int? Version { get; set; }
    public AccessType? AccessType { get; set; }
    public List<Guid>? Tags { get; set; }
    public FileStorageSqlTemplate(Guid uuid, string name, string displayLabel, string? description, DbType dbType,
        int version, AccessType accessType, List<Guid> tags)
    {
        Uuid = uuid;
        Name = name;
        DisplayLabel = displayLabel;
        Description = description;
        DbType = dbType;
        Version = version;
        AccessType = accessType;
        Tags = tags;
    }

    public FileStorageSqlTemplate()
    {
    }
}