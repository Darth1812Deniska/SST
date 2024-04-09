using SSTCommonClasses.Constants;
using SSTCommonClasses.Constructor.Models;

namespace SSTCommonClasses.Constructor.Tags;

[Serializable]
public class Tag : ConstructorItem
{
    #region Properties
    public TagType? TagType { get; set; }
    public TagDataType? DataType { get; set; }
    public TagTemplate? TagTemplate { get; set; }
    public SqlTemplate? SqlTemplate { get; set; }
    public bool? IsMultiplePattern { get; set; }

    #endregion


    #region Constructors

    public Tag()
    {
    }

    public Tag(string name)
    {
        Uuid = Guid.NewGuid();
        Name = name;
    }

    #endregion

    #region Public methods

    public void SetName(string name)
    {
        Name = name;
    }

    public void SetAccessType(AccessType accessType)
    {
        AccessType = accessType;
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

    public override int GetHashCode()
    {
        return HashCode.Combine(Uuid);
    }

    #endregion
}