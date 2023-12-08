namespace SSTCommonClasses.Models;

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

    public ModelItem(SqlTemplate sqlTemplate)
    {
        Uuid = Guid.NewGuid();
        SqlTemplate = sqlTemplate;
        ChildModelItems = new Dictionary<Guid, ModelItem>();
    }

    // Для создания rootmodelItem
    public ModelItem(string name, string displayLabel, string description, DbType dbType)
    {
        Uuid = Guid.NewGuid();
        SqlTemplate = new SqlTemplate(name, displayLabel, description, dbType);
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