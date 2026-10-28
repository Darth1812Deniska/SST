using System;
using SSTCommonClasses.Constants;
using SSTCommonClasses.Constructor.Tags;

namespace SqlScriptToolAvalonia.ConstructorStorage.FileStorageSerializeTemplates;

public class FileStorageTag
{
    public Guid? Uuid { get; set; }
    public string? Name { get; set; }
    public string? DisplayLabel { get; set; }
    public string? Description { get; set; }
    public TagType? TagType { get; set; }
    public TagDataType? DataType { get; set; }
    public Guid? TagTemplate { get; set; }
    public Guid? LdObjectType { get; set; }
    public Guid? SqlTemplate { get; set; }
    public AccessType? AccessType { get; set; }

    public FileStorageTag(Guid uuid, string name, string? displayLabel, string? description, TagType tagType,
        TagDataType? dataType, Guid? tagTemplate, Guid? ldObjectType, Guid? sqlTemplate, AccessType accessType)
    {
        Uuid = uuid;
        Name = name;
        DisplayLabel = displayLabel;
        Description = description;
        TagType = tagType;
        DataType = dataType;
        TagTemplate = tagTemplate;
        LdObjectType = ldObjectType;
        SqlTemplate = sqlTemplate;
        AccessType = accessType;
    }

    public FileStorageTag()
    {
    }
}