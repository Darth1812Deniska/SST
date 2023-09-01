using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
namespace SSTCommonClasses;

public enum TagDataType
{
    Int,
    Float,
    Date,
    DateTime,
    String,
    Char,
    LdObjectType
}

public enum TagType
{
    UserParameter,
    ModelItemparent
}

[Serializable]

public class TagTemplate
{
    
    public Guid Uuid { get; set; }
    public string Name { get; set; }
    public string DisplayLabel { get; set; }
    public string Description { get; set; }
    public TagType TagType { get; set; }
    public TagDataType DataType { get; set; }
    public LdObjectType? LdObjectType { get; set; }

    private TagTemplate()
    {
    }

    public TagTemplate(Guid? uuid, string name, string displayLabel, string description, TagType tagType,
        TagDataType dataType, LdObjectType? ldObjectType)
    {
        Uuid = uuid ?? Guid.NewGuid();
        Name = name;
        DisplayLabel = displayLabel;
        Description = description;
        TagType = tagType;
        DataType = dataType;
        LdObjectType = ldObjectType;
    }

    public TagTemplate(string name, string displayLabel, string description, TagType tagType, TagDataType dataType,
        LdObjectType? ldObjectType)
    {
        Uuid = Guid.NewGuid();
        Name = name;
        DisplayLabel = displayLabel;
        Description = description;
        TagType = tagType;
        DataType = dataType;
        LdObjectType = ldObjectType;
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

[Serializable]
public class Tag
{
    public Guid Uuid { get; set; }
    public string Name { get; set; }
    public string DisplayLabel { get; set; }
    public string Description { get; set; }
    public TagType TagType { get; set; }
    public TagDataType DataType { get; set; }
    public TagTemplate TagTemplate { get; set; }
    public LdObjectType? LdObjectType { get; set; }
    public ModelItem? ChildModelItem { get; set; }

    public Tag()
    {
    }

    public Tag(Guid? uuid, TagTemplate tagTemplate, string name, string displayLabel, string description,
        TagType tagType, TagDataType dataType, LdObjectType? ldObjectType, ModelItem? childModelItem)
    {
        Uuid = uuid ?? Guid.NewGuid();
        Name = name;
        DisplayLabel = displayLabel;
        Description = description;
        TagType = tagType;
        DataType = dataType;
        TagTemplate = tagTemplate;
        LdObjectType = ldObjectType;
        ChildModelItem = childModelItem;
    }

    public Tag(TagTemplate tagTemplate, string name, string displayLabel, string description, TagType tagType,
        TagDataType dataType, LdObjectType? ldObjectType, ModelItem? childModelItem)
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
    }

    public Tag(TagTemplate tagTemplate, LdObjectType? ldObjectType, ModelItem? childModelItem)
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