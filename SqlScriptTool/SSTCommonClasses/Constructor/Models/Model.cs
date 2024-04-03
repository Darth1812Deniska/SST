using SSTCommonClasses.Constants;


namespace SSTCommonClasses.Constructor.Models;

[Serializable]
public class Model : ConstructorItem
{
    private ModelItem _rootModelItem = null;
    private string _name;
    private string? _description;
    private DbType _dbType;
    private string _displayLabel;
    private string _creatorName;
    private int _version;

    public string Name
    {
        get => _name;
        set => _name = value;
    }

    public string? Description
    {
        get => _description;
        set => _description = value;
    }

    public string DisplayLabel
    {
        get => _displayLabel;
        set => _displayLabel = value;
    }

    public DbType DbType
    {
        get => _dbType;
        set => _dbType = value;
    }

    public string CreatorName
    {
        get => _creatorName;
        set => _creatorName = value;
    }



    public int Version
    {
        get => _version;
        set => _version = value;
    }

    public ModelItem RootModelItem
    {
        get => _rootModelItem ??= new ModelItem();
        set => _rootModelItem = value;
    }

    /// <summary>
    /// Создание новой модели с параметрами по-умолчанию
    /// </summary>
    public Model()
    {
    }


    public Model(Guid uuid, string name, string? description, DbType dbType, string displayLabel, string creatorName,
        AccessType accessType, int version, ModelItem rootModelItem)
    {
        Uuid = uuid;
        Name = name;
        Description = description;
        DbType = dbType;
        DisplayLabel = displayLabel;
        CreatorName = creatorName;
        AccessType = accessType;
        Version = version;
        _rootModelItem = rootModelItem;
    }

    public void InitRootModelItem()
    {
        _rootModelItem = new ModelItem()
        {
            SqlTemplate = new SqlTemplate()
        };
    }

    public bool Equals(Model? other)
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

        return obj is Model model && Equals(model);
    }

    public override string ToString()
    {
        return $"{Name} ({DisplayLabel})";
    }
}