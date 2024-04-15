using SSTCommonClasses.Constants;
using SSTCommonClasses.Constructor.Tags;

namespace SSTCommonClasses.Constructor.Models;


/// <summary>
/// Класс описывающий SQL-шаблоны, прикрпеленные к модели
/// </summary>
[Serializable]
public class SqlTemplate : ConstructorItem
{
    private List<Tag>? _tags = null;

    private string? _creator;
    private DbType _dbType;
    private int _version;
    private string? _sqlText;

    #region Properties

    /// <summary>
    /// Создатель SQL-шаблона
    /// </summary>
    public string? Creator
    {
        get => _creator;
        set => _creator = value;
    }

    /// <summary>
    /// Тип СУБД, для которой разрабатывается SQL-шаблон
    /// </summary>
    public DbType DbType
    {
        get => _dbType;
        set => _dbType = value;
    }

    /// <summary>
    /// Номер версии шаблона
    /// </summary>
    public int Version
    {
        get => _version;
        set => _version = value;
    }

    /// <summary>
    /// Текст SQL-шаблона
    /// </summary>
    public string? SqlText
    {
        get => _sqlText;
        set => _sqlText = value;
    }

    /// <summary>
    /// Список тэгов прикрпеленных к SQL-шаблону
    /// </summary>
    public List<Tag> Tags
    {
        get => _tags ??= new List<Tag>();
        set => _tags = value;
    }

    /// <summary>
    /// Метка, указывающая является ли SQL-шаблон множественным паттерном
    /// </summary>
    public bool IsMultiplePattern { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Создание эземпляра sql-шаблона
    /// </summary>
    public SqlTemplate()
    {
    }

    #endregion


    // Для создания Шаблона RootModelItem

    #region Public methods

    /// <summary>
    /// Изменение имени шаблона
    /// </summary>
    /// <param name="name">Новое имя шаблона</param>
    public void SetName(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Изменение отображаемого имени шаблона
    /// </summary>
    /// <param name="displayLabel">Новое отогбражаемое имя шаблона</param>
    public void SetDisplayLabel(string displayLabel)
    {
        DisplayLabel = displayLabel;
    }

    /// <summary>
    /// Изменение описание шаблона
    /// </summary>
    /// <param name="description">Новое описание шаблона</param>
    public void SetDescription(string? description)
    {
        Description = description;
    }

    /// <summary>
    /// Изменение типа СУБД шаблона
    /// </summary>
    /// <param name="dbType">Новый тип СУБД</param>
    public void SetDbType(DbType dbType)
    {
        DbType = dbType;
    }

    /// <summary>
    /// Изменение типа доступа
    /// </summary>
    /// <param name="accessType">Новый тип доступа</param>
    public void SetAccessType(AccessType accessType)
    {
        AccessType = accessType;
    }

    /// <summary>
    /// Добавление ссылочного тэга
    /// </summary>
    /// <param name="tagName">Название нового тэга</param>
    /// <param name="modelItem">Узел модели</param>
    /// <returns></returns>
    public Tag AddModelItemTag(string tagName, ModelItem modelItem)
    {
        Tag modelItemTag = new()
        {
            Name = tagName,
            TagType = TagType.ModelItemParent,
            SqlTemplate = modelItem.SqlTemplate,
            AccessType = AccessType
        };

        Tags.Add(modelItemTag);
        AddModelItemTagToScript(tagName);
        return modelItemTag;
    }

    /// <summary>
    /// Добавление имени ссылочного тэга в текст скрипта
    /// </summary>
    /// <param name="tagName">Название ссылочного тэга</param>
    public void AddModelItemTagToScript(string? tagName)
    {
        SqlText = string.IsNullOrEmpty(SqlText)
            ? tagName
            : string.Concat(SqlText, Environment.NewLine, tagName);
    }

    public bool Equals(SqlTemplate? other)
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

        return obj is SqlTemplate sqlTemplate && Equals(sqlTemplate);
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

    #region Private Methods

    #endregion
}