using SSTCommonClasses.Landocs;
using SSTCommonClasses.Models;

namespace SSTCommonClasses.Tags;

[Serializable]
public class Tag
{
    public Guid Uuid { get; set; }
    public string Name { get; set; }
    public string DisplayLabel { get; set; }
    public string Description { get; set; }
    public TagType TagType { get; set; }
    public TagDataType? DataType { get; set; }
    public TagTemplate? TagTemplate { get; set; }
    public LdObjectType? LdObjectType { get; set; }
    public ModelItem? ChildModelItem { get; set; }

    public bool IsMultiplePattern { get; set; }

    
    public Tag()
    {
    }

    public Tag(string name, string displayLabel, string description, TagType tagType, TagDataType? dataType,
        LdObjectType? ldObjectType, ModelItem? childModelItem, bool isMultiplePattern)
    {
        Uuid = Guid.NewGuid();
        Name = name;
        DisplayLabel = displayLabel;
        Description = description;
        TagType = tagType;
        DataType = dataType;
        LdObjectType = ldObjectType;
        ChildModelItem = childModelItem;
        IsMultiplePattern = isMultiplePattern;
    }

    public Tag(TagTemplate tagTemplate, string name, string displayLabel, string description, TagType tagType,
        TagDataType? dataType, LdObjectType? ldObjectType, ModelItem? childModelItem, bool isMultiplePattern)
    {
        Uuid = Guid.NewGuid();
        Name = name;
        DisplayLabel = displayLabel;
        Description = description;
        TagType = tagType;
        DataType = dataType;
        TagTemplate = tagTemplate;
        LdObjectType = ldObjectType;
        ChildModelItem = childModelItem;
        IsMultiplePattern = isMultiplePattern;
    }

    public Tag(TagTemplate tagTemplate, LdObjectType? ldObjectType, ModelItem? childModelItem, bool isMultiplePattern)
    {
        Uuid = Guid.NewGuid();
        Name = tagTemplate.Name;
        DisplayLabel = tagTemplate.DisplayLabel;
        Description = tagTemplate.Description;
        TagType = tagTemplate.TagType;
        DataType = tagTemplate.DataType;
        TagTemplate = tagTemplate;
        LdObjectType = ldObjectType;
        ChildModelItem = childModelItem;
        IsMultiplePattern = isMultiplePattern;
    }

    public bool Equals(Tag? other)
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

        return obj is Tag tag && Equals(tag);
    }

    public override string ToString()
    {
        return $"{Name} ({DisplayLabel})";
    }
}