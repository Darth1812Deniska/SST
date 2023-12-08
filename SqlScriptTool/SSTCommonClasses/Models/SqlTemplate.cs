using SSTCommonClasses.Tags;

namespace SSTCommonClasses.Models;

[Serializable]
public class SqlTemplate
{
    public Guid Uuid { get; set; }
    public string Name { get; set; }
    public string DisplayLabel { get; set; }
    public string Description { get; set; }
    public string Creator { get; set; }
    public DbType DbType { get; set; }
    public int Version { get; set; }
    public string SqlText { get; set; }
    public List<Tag> Tags { get; }

    public SqlTemplate()
    {
        Uuid = Guid.NewGuid();
        Name = string.Empty;
        DisplayLabel = string.Empty;
        Description = string.Empty;
        Creator = string.Empty;
        DbType = DbType.None;
        Version = 0;
        SqlText = string.Empty;
        Tags = new List<Tag>();
    }

    // Для создания Шаблона RootModelItem
    public SqlTemplate(string name, string displayLabel, string description, DbType dbType)
    {
        Uuid = Guid.NewGuid();
        Name = name;
        DisplayLabel = displayLabel;
        Description = description;
        Creator = string.Empty;
        DbType = dbType;
        Version = 0;
        SqlText = string.Empty;
        Tags = new List<Tag>();
    }

    public SqlTemplate(string name, string displayLabel, string description, string creator, DbType dbType,
        int version,
        string sqlText)
    {
        Uuid = Guid.NewGuid();
        Name = name;
        DisplayLabel = displayLabel;
        Description = description;
        Creator = creator;
        DbType = dbType;
        Version = version;
        SqlText = sqlText;
        Tags = new List<Tag>();
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

    public void SetName(string name)
    {
        Name = name;
    }

    public void SetDisplayLabel(string displayLabel)
    {
        DisplayLabel = displayLabel;
    }

    public override string ToString()
    {
        return $"{Name} ({DisplayLabel})";
    }
}