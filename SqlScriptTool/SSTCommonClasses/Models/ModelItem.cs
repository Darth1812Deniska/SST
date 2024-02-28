using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SSTCommonClasses.Constants;
using SSTCommonClasses.Tags;

namespace SSTCommonClasses.Models;

[Serializable]
public class ModelItem
{
    //
    private Dictionary<string, ModelItem> _childModelItems = null;
    private SqlTemplate _sqlTemplate = null;
    private Guid _uuid = Guid.Empty;

    #region properties

    [Key]
    public Guid Uuid
    {
        get => _uuid == Guid.Empty ? _uuid=Guid.NewGuid() : _uuid;
        set => _uuid = value;
    }

    public SqlTemplate SqlTemplate
    {
        get => _sqlTemplate ??= new SqlTemplate();
        set => _sqlTemplate = value;
    }

    [NotMapped]
    public Dictionary<string, ModelItem> ChildModelItems
    {
        get => _childModelItems ??= new Dictionary<string, ModelItem>();
        set => _childModelItems = value;
    }

    #endregion

    #region Construnctors

    public ModelItem()
    {
    }

    public ModelItem(SqlTemplate sqlTemplate)
    {
        Uuid = Guid.NewGuid();
        SqlTemplate = sqlTemplate;
        _childModelItems = new Dictionary<string, ModelItem>();
    }

    #endregion

    #region Public methods

    public ModelItem AddChildModelItem(string nodeName)
    {
        var childModelItem = new ModelItem();
        var newChildTag = SqlTemplate.AddModelItemTag(nodeName, childModelItem);
        ChildModelItems.Add(newChildTag.Name, childModelItem);
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