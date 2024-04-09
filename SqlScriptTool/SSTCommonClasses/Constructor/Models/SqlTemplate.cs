using SSTCommonClasses.Constants;
using SSTCommonClasses.Constructor.Tags;

namespace SSTCommonClasses.Constructor.Models;

[Serializable]
public class SqlTemplate : ConstructorItem
{
    private List<Tag>? _tags = null;

    private string? _creator;
    private DbType _dbType;
    private int _version;
    private string? _sqlText;

    #region Properties

    public string? Creator
    {
        get => _creator;
        set => _creator = value;
    }

    public DbType DbType
    {
        get => _dbType;
        set => _dbType = value;
    }

    public int Version
    {
        get => _version;
        set => _version = value;
    }

    public string? SqlText
    {
        get => _sqlText;
        set => _sqlText = value;
    }

    public List<Tag> Tags
    {
        get => _tags ??= new List<Tag>();
        set => _tags = value;
    }

    public bool IsMultiplePattern { get; set; }

    #endregion

    #region Constructors

    public SqlTemplate()
    {
    }

    #endregion


    // Для создания Шаблона RootModelItem

    #region Public methods

    public void SetName(string name)
    {
        Name = name;
    }

    public void SetDisplayLabel(string displayLabel)
    {
        DisplayLabel = displayLabel;
    }

    public void SetDescription(string? description)
    {
        Description = description;
    }

    public void SetDbType(DbType dbType)
    {
        DbType = dbType;
    }

    public void SetAccessType(AccessType accessType)
    {
        AccessType = accessType;
    }

    public Tag AddModelItemTag(string tagName, ModelItem modelItem)
    {
        Tag modelItemTag = new Tag
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