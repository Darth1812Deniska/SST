using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SSTCommonClasses;

public enum DbType
{
    MsSql,
    PostgreSql,
    None
}

public enum ModelAccessType
{
    Public,
    Common,
    Private
}

[Serializable]
public class Model
{
    public Guid Uuid { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DbType DbType { get; set; }
    public string DisplayLabel { get; set; }
    public string CreatorName { get; set; }
    public ModelAccessType AccessType { get; set; }
    public int Version { get; set; }
    public ModelItem RootModelItem { get; }

    public Model()
    {
        Uuid = Guid.NewGuid();
        Name = String.Empty;
        Description = String.Empty;
        DbType = DbType.None;
        DisplayLabel = string.Empty;
        CreatorName = String.Empty;
        AccessType = ModelAccessType.Private;
        Version = 0;
        RootModelItem = new ModelItem();
    }

    public Model(Guid? uuid, string name, string description, DbType dbType, string displayLabel, string creatorName,
        ModelAccessType accessType, int version)
    {
        Uuid = uuid ?? Guid.NewGuid();
        Name = name;
        Description = description;
        DbType = dbType;
        DisplayLabel = displayLabel;
        CreatorName = creatorName;
        AccessType = accessType;
        Version = version;
        RootModelItem = new ModelItem();
    }

    public Model(string name, string description, DbType dbType, string displayLabel, string creatorName,
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
        RootModelItem = new ModelItem();
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

    public SqlTemplate(Guid? uuid, string name, string displayLabel, string description, string creator, DbType dbType,
        int version, string sqlText)
    {
        Uuid = uuid ?? Guid.NewGuid();
        Name = name;
        DisplayLabel = displayLabel;
        Description = description;
        Creator = creator;
        DbType = dbType;
        Version = version;
        SqlText = sqlText;
        Tags = new List<Tag>();
    }

    public SqlTemplate(string name, string displayLabel, string description, string creator, DbType dbType, int version,
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

    public override string ToString()
    {
        return $"{Name} ({DisplayLabel})";
    }
}

[Serializable]
public class ModelItem
{
    public Guid Uuid { get; set; }
    public SqlTemplate SqlTemplate { get; set; }
    public Dictionary<Guid, ModelItem> ChildModelItems { get; }

    public ModelItem()
    {
        Uuid = Guid.NewGuid();
        SqlTemplate = new SqlTemplate();
        ChildModelItems = new Dictionary<Guid, ModelItem>();
    }

    public ModelItem(Guid? uuid, SqlTemplate sqlTemplate)
    {
        Uuid = uuid ?? Guid.NewGuid();
        SqlTemplate = sqlTemplate;
        ChildModelItems = new Dictionary<Guid, ModelItem>();
    }

    public bool Equals(ModelItem? other)
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

        return obj is ModelItem modelItem && Equals(modelItem);
    }

    public override string ToString()
    {
        return Uuid.ToString();
    }
}