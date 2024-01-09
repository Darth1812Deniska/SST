using SSTCommonClasses.Constants;
using SSTCommonClasses.Tags;
using System.Collections.Generic;
using System;

namespace SqlScriptToolClient.ConstructorStorage.FileStorageSerializeTemplates;

[Serializable]
public class FileStorageSqlTemplate
{
    public Guid Uuid { get; set; }
    public string Name { get; set; }
    public string DisplayLabel { get; set; }
    public string? Description { get; set; }
    public DbType DbType { get; set; }
    public int Version { get; set; }
    public string SqlText { get; set; }
    public AccessType AccessType { get; set; }
    public List<Tag> Tags { get; set; }
    public FileStorageSqlTemplate(Guid uuid, string name, string displayLabel, string? description, DbType dbType,
        int version, string sqlText, AccessType accessType, List<Tag> tags)
    {
        Uuid = uuid;
        Name = name;
        DisplayLabel = displayLabel;
        Description = description;
        DbType = dbType;
        Version = version;
        SqlText = sqlText;
        AccessType = accessType;
        Tags = tags;
    }

    public FileStorageSqlTemplate()
    {
    }
}