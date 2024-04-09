using SSTCommonClasses.Constructor.Models;

namespace SSTCommonClasses.Constructor.Tags;

[Serializable]
public class TagTemplate : ConstructorItem
{
    public TagType TagType { get; set; }
    public TagDataType? DataType { get; set; }
    public SqlTemplate? SqlTemplate { get; set; }
    public bool IsMultiplePattern { get; set; }

    public TagTemplate()
    {
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

    public override int GetHashCode()
    {
        return HashCode.Combine(Uuid);
    }
}