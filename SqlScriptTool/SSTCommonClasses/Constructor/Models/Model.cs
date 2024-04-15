using SSTCommonClasses.Constants;


namespace SSTCommonClasses.Constructor.Models;

/// <summary>
/// Класс описывающий модель
/// </summary>
[Serializable]
public class Model : ConstructorItem
{
    private ModelItem? _rootModelItem = null;
    private DbType _dbType;
    private string? _creatorName;
    private int _version;

    /// <summary>
    /// Тип СУБД, для которой разрабатывается модель
    /// </summary>
    public DbType DbType
    {
        get => _dbType;
        set => _dbType = value;
    }

    /// <summary>
    /// Разработчик модели
    /// </summary>
    public string? CreatorName
    {
        get => _creatorName;
        set => _creatorName = value;
    }

    /// <summary>
    /// Номер версии модели
    /// </summary>
    public int Version
    {
        get => _version;
        set => _version = value;
    }

    /// <summary>
    /// Корневой узел модели
    /// </summary>
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

    /// <summary>
    /// Создание новой модели с заданными параметрами
    /// </summary>
    /// <param name="uuid">Уникальный идентифкатор модели</param>
    /// <param name="name">Название модели</param>
    /// <param name="description">Описание модели</param>
    /// <param name="dbType">Тип СУБД, для которой разрабатывается модель</param>
    /// <param name="displayLabel">Отображаемое название модели</param>
    /// <param name="creatorName">Имя разработчика</param>
    /// <param name="accessType">Тип доступа к модели</param>
    /// <param name="version">Номер версии модели</param>
    /// <param name="rootModelItem">Корневой узел модели</param>
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

    public override int GetHashCode()
    {
        return HashCode.Combine(Uuid);
    }
}