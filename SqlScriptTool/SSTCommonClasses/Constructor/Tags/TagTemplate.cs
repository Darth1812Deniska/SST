using System.ComponentModel.DataAnnotations;
using SSTCommonClasses.Constructor;
using SSTCommonClasses.Constructor.Models;
using SSTCommonClasses.Landocs;

namespace SSTCommonClasses.Constructor.Tags;

[Serializable]
public class TagTemplate : ConstructorItem
{
    public TagType TagType { get; set; }
    public TagDataType? DataType { get; set; }
    public LdObjectType? LdObjectType { get; set; }
    public SqlTemplate? SqlTemplate { get; set; }
    public bool IsMultiplePattern { get; set; }

    public TagTemplate()
    {
    }


    public TagTemplate(string name, string? displayLabel, string? description, TagType tagType, TagDataType? dataType,
        LdObjectType? ldObjectType, SqlTemplate? sqlTemplate, bool isMultiplePattern)
    {
        Uuid = Guid.NewGuid();
        Name = name;
        DisplayLabel = displayLabel;
        Description = description;
        TagType = tagType;
        DataType = dataType;
        LdObjectType = ldObjectType;
        SqlTemplate = sqlTemplate;
        IsMultiplePattern = isMultiplePattern;
    }

    public bool Equals(TagTemplate? other)
    {
        if (other == null)
            return false;
        return Uuid == other.Uuid;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }

        return obj is TagTemplate tagTemplate && Equals(tagTemplate);
    }

    public override string ToString()
    {
        return $"{Name} ({DisplayLabel})";
    }
}