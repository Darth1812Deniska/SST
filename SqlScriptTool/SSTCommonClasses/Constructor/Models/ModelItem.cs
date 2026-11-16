using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SSTCommonClasses.Constants;
using SSTCommonClasses.Constructor.Tags;
using SSTCommonClasses.ConstructorStorage;


namespace SSTCommonClasses.Constructor.Models;

/// <summary>
/// Класс описывающий узлы модели.
/// Узел хранит только ссылку на sql-шаблон — его уникальный идентификатор
/// <see cref="SqlTemplateUuid"/>; сам текст и атрибуты шаблона живут в хранилище
/// (<see cref="ConstructorStorage.StaticConstructorStorage.SqlTemplates"/>) и получаются
/// через <see cref="GetSqlTemplate"/>. Такое разделение исключает появление дублей
/// sql-шаблонов при редактировании модели: повторное сохранение обновляет существующий
/// шаблон по его уникальному идентификатору, а не создаёт новый.
/// </summary>
[Serializable]
public class ModelItem
{
    //
    private Dictionary<string, ModelItem>? _childModelItems = null;
    private Guid _uuid = Guid.Empty;
    private Guid _sqlTemplateUuid = Guid.Empty;

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
    /// Уникальный идентификатор sql-шаблона, на который ссылается узел.
    /// Guid.Empty — если шаблон ещё никогда не был создан/присоединён (ленивое состояние).
    /// Присвоение Guid.Empty снимает ссылку на шаблон с узла.
    /// Сам шаблон по этой ссылке получается методом <see cref="GetSqlTemplate"/>.
    /// </summary>
    public Guid SqlTemplateUuid
    {
        get => _sqlTemplateUuid;
        set => _sqlTemplateUuid = value;
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
    /// Создание узла модели без ссылки на sql-шаблон
    /// </summary>
    public ModelItem()
    {
    }

    /// <summary>
    /// Создание узла модели со ссылкой на уже существующий sql-шаблон
    /// </summary>
    /// <param name="sqlTemplateUuid">Уникальный идентификатор sql-шаблона, на который ссылается узел</param>
    public ModelItem(Guid sqlTemplateUuid)
    {
        Uuid = Guid.NewGuid();
        _sqlTemplateUuid = sqlTemplateUuid;
        _childModelItems = new Dictionary<string, ModelItem>();
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Получение sql-шаблона, на который ссылается данный узел (<see cref="SqlTemplateUuid"/>).
    /// Шаблон берётся из уже загруженных в систему объектов хранилища, поэтому метод
    /// возвращает null, если ссылка отсутствует (Uuid не назначен) либо шаблон с таким
    /// уникальным идентификатором в систему ещё не выгружен.
    /// </summary>
    /// <returns>Найденный sql-шаблон либо null</returns>
    public SqlTemplate? GetSqlTemplate()
    {
        return GetSqlTemplate(SqlTemplateUuid);
    }

    /// <summary>
    /// Получение sql-шаблона, уже выгруженного в систему, по его уникальному идентификатору.
    /// Используется везде, где узлу модели нужен текст или атрибуты шаблона: сам узел хранит
    /// только ссылку (<see cref="SqlTemplateUuid"/>), что исключает появление дублей шаблонов
    /// после редактирования модели.
    /// </summary>
    /// <param name="sqlTemplateUuid">Уникальный идентификатор sql-шаблона</param>
    /// <returns>Найденный sql-шаблон либо null, если ссылка не задана или шаблон в систему не выгружен</returns>
    public static SqlTemplate? GetSqlTemplate(Guid sqlTemplateUuid)
    {
        if (sqlTemplateUuid == Guid.Empty)
        {
            return null;
        }

        return StaticConstructorStorage.SqlTemplates.GetById(sqlTemplateUuid);
    }

    /// <summary>
    /// Регистрация sql-шаблона узла в хранилище (в памяти и в файловой системе), если он там ещё отсутствует.
    /// Нужна потому, что узел хранит только ссылку на шаблон: новый шаблон, созданный при редактировании,
    /// должен быть выгружен в систему до первого обращения по ссылке. Если шаблон с таким уникальным
    /// идентификатором уже выгружен — повторная запись не выполняется, то есть дубль не создаётся.
    /// </summary>
    /// <param name="sqlTemplate">Регистрируемый sql-шаблон</param>
    public void RegisterSqlTemplate(SqlTemplate sqlTemplate)
    {
        SqlTemplateUuid = sqlTemplate.Uuid;
        if (StaticConstructorStorage.SqlTemplates.GetById(sqlTemplate.Uuid) == null)
        {
            StaticConstructorStorage.SaveSqlTemplate(sqlTemplate);
        }
    }

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

        // Новый дочерний узел сразу создается со ссылкой на собственный sql-шаблон,
        // который регистрируется в хранилище (повторной записью одного и того же
        // шаблона дубли не образуются)
        SqlTemplate childSqlTemplate = new();
        ModelItem childModelItem = new(childSqlTemplate.Uuid);
        childModelItem.RegisterSqlTemplate(childSqlTemplate);

        Tag newChildTag = AddModelItemTagToOwnTemplate(tagNodeName, childModelItem);
        childModelItem.SetDbType(dbType);
        RenameOwnTemplate(newChildTag.Name);
        ChildModelItems[newChildTag.Name] = childModelItem;
        return childModelItem;
    }

    /// <summary>
    /// Добавление ссылочного тэга на дочерний узел в sql-шаблон текущего узла.
    /// Если шаблон текущего узла ещё не создан (ссылка пустая), он создается и регистрируется в хранилище.
    /// </summary>
    /// <param name="tagName">Имя тэга дочернего узла</param>
    /// <param name="childModelItem">Дочерний узел, на который ссылается тэг</param>
    /// <returns>Созданный (либо уже существовавший) тэг</returns>
    public Tag AddModelItemTagToOwnTemplate(string tagName, ModelItem childModelItem)
    {
        SqlTemplate ownTemplate = EnsureOwnTemplate();
        return ownTemplate.AddModelItemTag(tagName, childModelItem);
    }

    /// <summary>
    /// Изменение имени sql-шаблона текущего узла. Если шаблон ещё не создан, он создается
    /// и регистрируется в хранилище.
    /// </summary>
    /// <param name="name">Новое имя sql-шаблона</param>
    public void RenameOwnTemplate(string name)
    {
        EnsureOwnTemplate().SetName(name);
    }

    /// <summary>
    /// Установка типа доступа sql-шаблона текущего узла. Если шаблон ещё не создан,
    /// он создается и регистрируется в хранилище.
    /// </summary>
    /// <param name="accessType">Новый тип доступа</param>
    public void SetOwnTemplateAccessType(AccessType accessType)
    {
        EnsureOwnTemplate().SetAccessType(accessType);
    }

    /// <summary>
    /// Возврат sql-шаблона текущего узла; если ссылка ещё не назначена — создание нового шаблона
    /// и его регистрация в хранилище. Используется при редактировании атрибутов шаблона узла
    /// (например, переименование модели по вводу текста в поле «Название» окна создания модели).
    /// Повторные вызовы всегда возвращают один и тот же объект (по ссылке узла), поэтому
    /// новые экземпляры шаблонов не плодятся.
    /// </summary>
    /// <returns>Sql-шаблон текущего узла</returns>
    public SqlTemplate EnsureOwnTemplate()
    {
        SqlTemplate? template = GetSqlTemplate();
        if (template != null)
        {
            return template;
        }

        SqlTemplate newTemplate = new();
        RegisterSqlTemplate(newTemplate);
        return newTemplate;
    }

    /// <summary>
    /// Установление типа СУБД для sql-шаблонов данного узла и всех дочерних узлов
    /// </summary>
    /// <param name="dbType"></param>
    public void SetDbType(DbType dbType)
    {
        GetSqlTemplate()?.SetDbType(dbType);
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
    /// Создание узла модели со ссылкой на sql-шаблон по уникальному идентификатору.
    /// Используется при загрузке DTO из файловой системы: сам шаблон будет получен
    /// из хранилища по ссылке методом <see cref="GetSqlTemplate(Guid)"/>.
    /// </summary>
    /// <param name="uuid">Уникальный идентификатор узла</param>
    /// <param name="sqlTemplateUuid">Уникальный идентификатор сохраненного sql-шаблона</param>
    public static ModelItem CreateWithTemplateReference(Guid uuid, Guid sqlTemplateUuid)
    {
        ModelItem modelItem = new(sqlTemplateUuid) { Uuid = uuid };
        return modelItem;
    }

    #endregion
}