using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SSTCommonClasses.Constants;
using SSTCommonClasses.Constructor.Tags;


namespace SSTCommonClasses.Constructor.Models;

/// <summary>
/// Класс описывающий узлы модели
/// </summary>
[Serializable]
public class ModelItem
{
    //
    private Dictionary<string, ModelItem>? _childModelItems = null;
    private SqlTemplate? _sqlTemplate = null;
    private Guid _uuid = Guid.Empty;

    #region properties

    /// <summary>
    /// Уникальный идентификатор узла
    /// </summary>
    [Key]
    public Guid Uuid
    {
        get => _uuid == Guid.Empty ? _uuid = Guid.NewGuid() : _uuid;
        set => _uuid = value;
    }

    /// <summary>
    /// SQL-шаблон прикрепленный к узлу
    /// </summary>
    public SqlTemplate SqlTemplate
    {
        get => _sqlTemplate ??= new SqlTemplate();
        set => _sqlTemplate = value;
    }

    /// <summary>
    /// Дочерние узлы
    /// </summary>
    [NotMapped]
    public Dictionary<string, ModelItem> ChildModelItems
    {
        get => _childModelItems ??= new Dictionary<string, ModelItem>();
        set => _childModelItems = value;
    }

    #endregion

    #region Construnctors

    /// <summary>
    /// Создание узла модели
    /// </summary>
    public ModelItem()
    {
    }

    /// <summary>
    /// Создание узла модели с прикрепленным SQL-шаблоном
    /// </summary>
    /// <param name="sqlTemplate">Прикрепляемый SQL-шаблон</param>
    public ModelItem(SqlTemplate sqlTemplate)
    {
        Uuid = Guid.NewGuid();
        SqlTemplate = sqlTemplate;
        _childModelItems = new Dictionary<string, ModelItem>();
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Добавление нового дочернего узла с заданным именем
    /// </summary>
    /// <param name="nodeName">Имя узла</param>
    /// <param name="dbType">Тип СУБД</param>
    /// <returns>Созданный дочерний узел</returns>
    public ModelItem AddChildModelItem(string nodeName, DbType dbType)
    {
        ModelItem childModelItem = new ();
        Tag newChildTag = SqlTemplate.AddModelItemTag(nodeName, childModelItem);
        childModelItem.SetDbType(dbType);
        childModelItem.SqlTemplate.SetName(nodeName);
        ChildModelItems.Add(newChildTag.Name, childModelItem);
        return childModelItem;
    }

    /// <summary>
    /// Установление типа СУБД для привязанных SQL-шаблонов
    /// </summary>
    /// <param name="dbType"></param>
    public void SetDbType(DbType dbType)
    {
        SqlTemplate.SetDbType(dbType);
        foreach (KeyValuePair<string, ModelItem> childModelItem in ChildModelItems)
        {
            childModelItem.Value.SetDbType(dbType);
        }
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

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    #endregion
}