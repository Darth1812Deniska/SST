using SSTCommonClasses.Infrastructure;

namespace SSTCommonClasses.Landocs;

[Serializable]
public class LdObjectType
{
    private int _id;
    private string _name;
    private string _displayLabel;
    private string? _description;
    private Guid _dbUid;
    private Project _project;
    private Guid _sstGuid;

    public int ID
    {
        get => _id;
        set => _id = value;
    }

    public string Name
    {
        get => _name;
        set => _name = value;
    }

    public string DisplayLabel
    {
        get => _displayLabel;
        set => _displayLabel = value;
    }

    public string? Description
    {
        get => _description;
        set => _description = value;
    }

    public Guid DbUid
    {
        get => _dbUid;
        set => _dbUid = value;
    }

    public Project Project
    {
        get => _project;
        set => _project = value;
    }

    public Guid SstGuid
    {
        get => _sstGuid;
        set => _sstGuid = value;
    }


    public LdObjectType()
    {
    }

    public bool Equals(LdObjectType? other)
    {
        if (other == null)
            return false;
        return ID == other.ID;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }

        return obj is LdObjectType ldObjectType && Equals(ldObjectType);
    }

    public override string ToString()
    {
        return $"{ID} - {Name} ({DisplayLabel})";
    }
}