using SSTCommonClasses.Tags;

namespace SSTCommonClasses.Models;

[Serializable]
public class Model
{
    public Guid Uuid { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public DbType DbType { get; set; }
    public string DisplayLabel { get; set; }
    public string CreatorName { get; set; }
    public ModelAccessType AccessType { get; set; }
    public int Version { get; set; }
    public ModelItem RootModelItem { get; }

    /// <summary>
    /// Создание новой модели с параметрами по-умолчанию
    /// </summary>
    public Model()
    {
        Uuid = Guid.NewGuid();
        Name = string.Empty;
        Description = string.Empty;
        DbType = DbType.None;
        DisplayLabel = string.Empty;
        CreatorName = string.Empty;
        AccessType = ModelAccessType.Private;
        Version = 0;
        SqlTemplate rootSqlTemplate =
            new SqlTemplate(Name, DisplayLabel, Description, CreatorName, DbType, Version, @"<script>");
        RootModelItem = new ModelItem(rootSqlTemplate);
    }

    /// <summary>
    /// Создание новой модели с задаными параметрами
    /// </summary>
    /// <param name="name">Физическое наименование модели</param>
    /// <param name="displayLabel">Отображаемое пользователсьское наименование</param>
    /// <param name="description">Описание модели</param>
    /// <param name="dbType">Тип СУБД, для которой создается модель</param>
    /// <param name="creatorName">Имя пользователя</param>
    /// <param name="accessType">Доступность модели</param>
    /// <param name="version">Версия</param>
    public Model(string name, string displayLabel, string description, DbType dbType, string creatorName,
        ModelAccessType accessType, int version)
    {
        Uuid = Guid.NewGuid();
        Name = name;
        Description = description;
        DbType = dbType;
        DisplayLabel = displayLabel;
        CreatorName = creatorName;
        AccessType = accessType;
        Version = version;
        SqlTemplate rootSqlTemplate =
            new SqlTemplate(name, displayLabel, description, creatorName, dbType, version, @"<script>");
        rootSqlTemplate.Tags.Add(new Tag("script", "Тело скрипта", "Тело скрипта", TagType.ModelItemParent, null, null,
            null));
        RootModelItem = new ModelItem(rootSqlTemplate);
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