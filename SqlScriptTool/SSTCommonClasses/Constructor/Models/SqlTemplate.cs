using SSTCommonClasses.Constants;
using SSTCommonClasses.ConstructorStorage;
using SSTCommonClasses.Constructor.Tags;

namespace SSTCommonClasses.Constructor.Models;


/// <summary>
/// Класс описывающий SQL-шаблоны, прикрпеленные к модели
/// </summary>
[Serializable]
public class SqlTemplate : ConstructorItem
{
    private List<Tag>? _tags = null;

    private string? _creator;
    private DbType _dbType;
    private int _version;
    private string? _sqlText;

    #region Properties

    /// <summary>
    /// Создатель SQL-шаблона
    /// </summary>
    public string? Creator
    {
        get => _creator;
        set => _creator = value;
    }

    /// <summary>
    /// Тип СУБД, для которой разрабатывается SQL-шаблон
    /// </summary>
    public DbType DbType
    {
        get => _dbType;
        set => _dbType = value;
    }

    /// <summary>
    /// Номер версии шаблона
    /// </summary>
    public int Version
    {
        get => _version;
        set => _version = value;
    }

    /// <summary>
    /// Текст SQL-шаблона
    /// </summary>
    public string? SqlText
    {
        get => _sqlText;
        set => _sqlText = value;
    }

    /// <summary>
    /// Список тэгов прикрпеленных к SQL-шаблону
    /// </summary>
    public List<Tag> Tags
    {
        get => _tags ??= new List<Tag>();
        set => _tags = value;
    }

    /// <summary>
    /// Метка, указывающая является ли SQL-шаблон множественным паттерном
    /// </summary>
    public bool IsMultiplePattern { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Создание эземпляра sql-шаблона
    /// </summary>
    public SqlTemplate()
    {
    }

    #endregion


    // Для создания Шаблона RootModelItem

    #region Public methods

    /// <summary>
    /// Изменение имени шаблона. Дополнительно обновляются имена тэгов ModelItemParent,
    /// представляющих данный шаблон в родительских шаблонах (связь выполняется по uuid
    /// sql-шаблона, а не по имени или ссылке на объект), и заменяется старый тэг в текстах
    /// этих родительских шаблонов. Это сохраняет согласованность имён «тэг ↔ дочерний
    /// sql-шаблон ↔ узел дерева» при переименовании шаблона из любого места.
    /// </summary>
    /// <param name="name">Новое имя шаблона</param>
    public void SetName(string name)
    {
        string oldName = Name;
        Name = name;

        if (string.IsNullOrEmpty(oldName) || oldName == name)
        {
            return;
        }

        RenameLinkedTagsEverywhere(name, oldName);
    }

    /// <summary>
    /// Переименование тэгов ModelItemParent, ссылающихся на данный шаблон (по uuid),
    /// во всех загруженных в хранилище sql-шаблонах, с заменой старого имени тэга
    /// в их текстах. Вызывается из <see cref="SetName"/>.
    /// </summary>
    private void RenameLinkedTagsEverywhere(string newName, string oldName)
    {
        foreach (SqlTemplate parentCandidate in StaticConstructorStorage.SqlTemplates)
        {
            // Сам переименовываемый шаблон не обрабатывается
            if (ReferenceEquals(parentCandidate, this))
            {
                continue;
            }

            bool renamed = false;
            foreach (Tag parentTag in parentCandidate.Tags)
            {
                if (parentTag.TagType != TagType.ModelItemParent)
                {
                    continue;
                }

                // Сопоставление ЯВНО по uuid sql-шаблона, на который ссылается тэг
                if (parentTag.SqlTemplateUuid is Guid tagTemplateUuid && tagTemplateUuid == Uuid)
                {
                    parentTag.Name = newName;
                    renamed = true;
                }
            }

            if (renamed && !string.IsNullOrEmpty(parentCandidate.SqlText)
                          && parentCandidate.SqlText.Contains(oldName, StringComparison.Ordinal))
            {
                parentCandidate.SqlText = parentCandidate.SqlText.Replace(oldName, newName);
            }
        }
    }

    /// <summary>
    /// Изменение отображаемого имени шаблона
    /// </summary>
    /// <param name="displayLabel">Новое отогбражаемое имя шаблона</param>
    public void SetDisplayLabel(string displayLabel)
    {
        DisplayLabel = displayLabel;
    }

    /// <summary>
    /// Изменение описание шаблона
    /// </summary>
    /// <param name="description">Новое описание шаблона</param>
    public void SetDescription(string? description)
    {
        Description = description;
    }

    /// <summary>
    /// Изменение типа СУБД шаблона
    /// </summary>
    /// <param name="dbType">Новый тип СУБД</param>
    public void SetDbType(DbType dbType)
    {
        DbType = dbType;
    }

    /// <summary>
    /// Изменение типа доступа
    /// </summary>
    /// <param name="accessType">Новый тип доступа</param>
    public void SetAccessType(AccessType accessType)
    {
        AccessType = accessType;
    }

    /// <summary>
    /// Добавление ссылочного тэга.
    /// Если тэг с таким именем уже присутствует в списке тэгов шаблона —
    /// существующий тэг возвращается без изменения (идемпотентность: повторное
    /// добавление не создаёт дубль и не изменяет текст скрипта).
    /// </summary>
    /// <param name="tagName">Название нового тэга</param>
    /// <param name="modelItem">Узел модели</param>
    /// <returns></returns>
    public Tag AddModelItemTag(string tagName, ModelItem modelItem)
    {
        // Идемпотентность: если такой тэг уже есть — возвращаем существующий,
        // не создавая дубль и не добавляя имя тэга в текст скрипта повторно
        Tag? existingTag = Tags.FirstOrDefault(tag => tag.Name == tagName);
        if (existingTag != null)
        {
            return existingTag;
        }

        Tag modelItemTag = new()
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

    /// <summary>
    /// Добавление имени ссылочного тэга в текст скрипта.
    /// Если тэг уже присутствует в тексте — дублирование не выполняется
    /// (иначе при переоткрытии шаблона один и тот же тэг добавлялся бы повторно).
    /// </summary>
    /// <param name="tagName">Название ссылочного тэга</param>
    public void AddModelItemTagToScript(string? tagName)
    {
        if (string.IsNullOrEmpty(tagName))
        {
            return;
        }

        // Тэг уже есть в тексте скрипта — ничего не добавляем
        if (!string.IsNullOrEmpty(SqlText) && SqlText.Contains(tagName, StringComparison.Ordinal))
        {
            return;
        }

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