using SSTCommonClasses.Constants;

namespace SSTCommonClasses.Models;

[Serializable]
public class ModelItem
{
    #region properties

    public Guid Uuid { get; set; }

    public SqlTemplate SqlTemplate { get; set; }
    public Dictionary<Guid, ModelItem> ChildModelItems { get; }

    #endregion

    #region Construnctors

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

    #endregion

    #region Public methods

    public ModelItem AddChildModelItem(string nodeName)
    {
        var childModelItem = new ModelItem();
        Guid newChildTagGuid = SqlTemplate.AddModelItemTag(nodeName, childModelItem);
        ChildModelItems.Add(newChildTagGuid, childModelItem);
        return childModelItem;
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

    #endregion
}