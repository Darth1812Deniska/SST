using System;
using SSTCommonClasses.Constructor.Tags;

namespace SqlScriptToolAvalonia.ConstructorStorage.FileStorageSerializeTemplates;

[Serializable]
public class FileStorageTagTemplate
{
    public Guid? Uuid { get; set; }
    public string? Name { get; set; }
    public string? DisplayLabel { get; set; }
    public string? Description { get; set; }
    public TagType? TagType { get; set; }
    public TagDataType? DataType { get; set; }
    public Guid? LdObjectType { get; set; } //SstGuid
    public Guid? SqlTemplate { get; set; }

    public FileStorageTagTemplate(Guid uuid, string name, string? displayLabel, string? description, TagType tagType,
        TagDataType? dataType, Guid ldObjectType, Guid sqlTemplate)
    {
        Uuid = uuid;
        Name = name;
        DisplayLabel = displayLabel;
        Description = description;
        TagType = tagType;
        DataType = dataType;
        LdObjectType = ldObjectType;
        SqlTemplate = sqlTemplate;
    }

    public FileStorageTagTemplate()
    {
    }
}