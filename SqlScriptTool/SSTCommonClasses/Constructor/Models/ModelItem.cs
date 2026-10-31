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
    private Guid? _savedSqlTemplateUuid = null;

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
    /// SQL-шаблон прикрепленный к узлу.
    /// Равен null, если шаблон еще не присоединен либо ссылка на сохраненный шаблон
    /// (по уникальному идентификатору) еще не разрешена при загрузке из файловой системы.
    /// Для безопасного обращения без проверки на null используйте <see cref="EffectiveSqlTemplate"/>.
    /// Присвоение null снимает шаблон с узла.
    /// </summary>
    public SqlTemplate? SqlTemplate
    {
        get => _sqlTemplate;
        set
        {
            _sqlTemplate = value;
            _savedSqlTemplateUuid = null;
        }
    }

    /// <summary>
    /// SQL-шаблон узла с ленивой инициализацией: если шаблон ещё не присоединён,
    /// он создаётся автоматически при первом обращении. Это позволяет редактировать
    /// атрибуты корневого шаблона сразу после открытия окна создания модели
    /// (например, переименование модели по вводу текста в поле «Название»).
    /// Исключение — неразрешенная ссылка на сохраненный шаблон после загрузки DTO
    /// из файловой системы: пока ссылка не разрешена вторым этапом выгрузки,
    /// возвращается null, чтобы новый пустой шаблон не затёр сохраненную связь.
    /// </summary>
    public SqlTemplate EffectiveSqlTemplate
    {
        get
        {
            if (_sqlTemplate == null && !HasSavedSqlTemplateReference)
            {
                _sqlTemplate = new SqlTemplate();
            }

            return _sqlTemplate!;
        }
    }

    /// <summary>
    /// Уникальный идентификатор sql-шаблона, на который ссылается узел после загрузки
    /// DTO из файловой системы, но до разрешения ссылки (второй этап выгрузки).
    /// Guid.Empty — если ссылка отсутствовала в сохраненных данных.
    /// </summary>
    [NotMapped]
    public Guid SavedSqlTemplateUuid => _savedSqlTemplateUuid ?? Guid.Empty;

    /// <summary>
    /// Признак наличия неразрешенной ссылки узла на сохраненный sql-шаблон
    /// </summary>
    [NotMapped]
    public bool HasSavedSqlTemplateReference => _savedSqlTemplateUuid.HasValue && _savedSqlTemplateUuid.Value != Guid.Empty;

    /// <summary>
    /// Установка ссылки узла на sql-шаблон по уникальному идентификатору (без разрешения ссылки).
    /// Используется на первом этапе выгрузки DTO из файловой системы; разрешение выполняется
    /// вторым этапом по уже загруженным в память объектам.
    /// </summary>
    /// <param name="sqlTemplateUuid">Уникальный идентификатор sql-шаблона</param>
    public void SetSqlTemplateReference(Guid sqlTemplateUuid)
    {
        _savedSqlTemplateUuid = sqlTemplateUuid;
        _sqlTemplate = null;
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
        // Имя ключа словаря должно совпадать с именем тэга в тексте шаблона:
        // если имя уже обрамлено угловыми скобками (<...>), используем его как есть,
        // иначе — оборачиваем, чтобы ключ ChildModelItems, имя тэга и имя sql-шаблона
        // дочернего узла были идентичны (иначе тэг не распознается в тексте шаблона)
        string tagNodeName = nodeName.StartsWith('<') && nodeName.EndsWith('>')
            ? nodeName
            : $"<{nodeName}>";

        // Новый дочерний узел сразу создается с собственным sql-шаблоном
        ModelItem childModelItem = new(new SqlTemplate());
        Tag newChildTag = EffectiveSqlTemplate.AddModelItemTag(tagNodeName, childModelItem);
        childModelItem.SetDbType(dbType);
        childModelItem.EffectiveSqlTemplate.SetName(newChildTag.Name);
        ChildModelItems[newChildTag.Name] = childModelItem;
        return childModelItem;
    }

    /// <summary>
    /// Установление типа СУБД для привязанных SQL-шаблонов
    /// </summary>
    /// <param name="dbType"></param>
    public void SetDbType(DbType dbType)
    {
        EffectiveSqlTemplate.SetDbType(dbType);
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

    /// <summary>
    /// Создание узла модели с неразрешенной ссылкой на sql-шаблон по уникальному идентификатору.
    /// Используется при загрузке DTO из файловой системы (первый этап выгрузки):
    /// ссылка разрешается вторым этапом по уже загруженным в память объектам.
    /// </summary>
    /// <param name="uuid">Уникальный идентификатор узла</param>
    /// <param name="sqlTemplateUuid">Уникальный идентификатор сохраненного sql-шаблона</param>
    public static ModelItem CreateWithTemplateReference(Guid uuid, Guid sqlTemplateUuid)
    {
        ModelItem modelItem = new() { Uuid = uuid };
        modelItem.SetSqlTemplateReference(sqlTemplateUuid);
        return modelItem;
    }

    #endregion
}