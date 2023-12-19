using SSTCommonClasses.Constants;
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
    public AccessType AccessType { get; set; }
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
        AccessType = AccessType.Private;
        Version = 0;
        SqlTemplate rootSqlTemplate =
            new SqlTemplate(Name, DisplayLabel, Description, CreatorName, DbType, AccessType, Version, @"<script>");
        RootModelItem = new ModelItem(rootSqlTemplate);
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
        RootModelItem = rootModelItem;
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