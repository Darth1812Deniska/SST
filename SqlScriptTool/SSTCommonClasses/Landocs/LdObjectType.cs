using SSTCommonClasses.Infrastructure;

namespace SSTCommonClasses.Landocs;

/// <summary>
/// Класс для хранения информации об объектах Ландокса
/// </summary>
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

    /// <summary>
    /// Идентификатор типа объекта Ландокс на конкретном проекте
    /// </summary>
    public int ID
    {
        get => _id;
        set => _id = value;
    }

    /// <summary>
    /// Название типа объекта Ландокс
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = value;
    }

    /// <summary>
    /// Отображаемое наименование объекта Ландокс
    /// </summary>
    public string DisplayLabel
    {
        get => _displayLabel;
        set => _displayLabel = value;
    }

    /// <summary>
    /// Краткое описание объекта
    /// </summary>
    public string? Description
    {
        get => _description;
        set => _description = value;
    }

    /// <summary>
    /// Uid типа объекта в рамках проекта
    /// </summary>
    public Guid DbUid
    {
        get => _dbUid;
        set => _dbUid = value;
    }

    /// <summary>
    /// Проект в котором используется этот тип объекта  
    /// </summary>
    public Project Project
    {
        get => _project;
        set => _project = value;
    }

    /// <summary>
    /// Уникальный идентификатор типа объекта в системе SST
    /// </summary>
    public Guid SstGuid
    {
        get => _sstGuid;
        set => _sstGuid = value;
    }


    /// <summary>
    /// Конструктор объекта класса LdObjectType
    /// </summary>
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