using SSTCommonClasses.Constants;
using SSTCommonClasses.Constructor.Models;

namespace SSTCommonClasses.Constructor.Tags;

/// <summary>
/// Класс описывающий тэги sql-шаблонов
/// </summary>
[Serializable]
public class Tag : ConstructorItem
{
    #region Properties
    /// <summary>
    /// Тип тэга
    /// </summary>
    public TagType? TagType { get; set; }
    /// <summary>
    /// Тип данных тэга пользовательских параметров
    /// </summary>
    public TagDataType? DataType { get; set; }
    /// <summary>
    /// Шаблон тэга, на основе которого создан текущий тэг.
    /// Равен null, если шаблон не привязан либо ссылка на сохраненный шаблон
    /// (по уникальному идентификатору) еще не разрешена при загрузке из файловой системы.
    /// </summary>
    public TagTemplate? TagTemplate
    {
        get => _tagTemplate;
        set
        {
            _tagTemplate = value;
            _savedTagTemplateUuid = null;
        }
    }

    /// <summary>
    /// Дочерний sql-щаблон привязанный к тэгу.
    /// Равен null, если шаблон не привязан либо ссылка на сохраненный шаблон
    /// (по уникальному идентификатору) еще не разрешена при загрузке из файловой системы.
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

    private TagTemplate? _tagTemplate;
    private SqlTemplate? _sqlTemplate;
    private Guid? _savedTagTemplateUuid;
    private Guid? _savedSqlTemplateUuid;

    /// <summary>
    /// Уникальный идентификатор шаблона тэга, на который ссылается тэг
    /// (в том числе неразрешенная ссылка, сохраненная при первом этапе выгрузки).
    /// null — если ссылка отсутствовала в сохраненных данных.
    /// </summary>
    public Guid? TagTemplateUuid => _tagTemplate?.Uuid ?? _savedTagTemplateUuid;

    /// <summary>
    /// Уникальный идентификатор дочернего sql-шаблона, на который ссылается тэг
    /// (в том числе неразрешенная ссылка, сохраненная при первом этапе выгрузки).
    /// null — если ссылка отсутствовала в сохраненных данных.
    /// </summary>
    public Guid? SqlTemplateUuid => _sqlTemplate?.Uuid ?? _savedSqlTemplateUuid;

    /// <summary>
    /// Установка ссылки тэга на шаблон тэга по уникальному идентификатору (без разрешения ссылки).
    /// Используется на первом этапе выгрузки DTO из файловой системы; разрешение выполняется
    /// вторым этапом по уже загруженным в память объектам.
    /// </summary>
    /// <param name="tagTemplateUuid">Уникальный идентификатор шаблона тэга</param>
    public void SetTagTemplateReference(Guid tagTemplateUuid)
    {
        _tagTemplate = null;
        _savedTagTemplateUuid = tagTemplateUuid;
    }

    /// <summary>
    /// Установка ссылки тэга на дочерний sql-шаблон по уникальному идентификатору
    /// (без разрешения ссылки). Используется на первом этапе выгрузки DTO из файловой системы;
    /// разрешение выполняется вторым этапом по уже загруженным в память объектам.
    /// </summary>
    /// <param name="sqlTemplateUuid">Уникальный идентификатор sql-шаблона</param>
    public void SetSqlTemplateReference(Guid sqlTemplateUuid)
    {
        _sqlTemplate = null;
        _savedSqlTemplateUuid = sqlTemplateUuid;
    }
    /// <summary>
    /// Метка, указывающая, является ли тэг родителем для множественного паттерна
    /// </summary>
    public bool? IsMultiplePattern { get; set; }
    
    /// <summary>
    /// Порядковый номер тега в шаблне
    /// </summary>
    /// 
    public int? Order { get; set; }
    
    /// <summary>
    /// Значение по умолчанию для тега. Может отсутствовать 
    /// </summary>
    public object? DefaultValue { get; set; }

    #endregion


    #region Constructors

    /// <summary>
    /// Создание нового экземпляра тэга
    /// </summary>
    public Tag()
    {
    }

    /// <summary>
    /// Создание нового экземпляра тэга с заданным именем
    /// </summary>
    /// <param name="name">Имя тэга</param>
    public Tag(string name)
    {
        Uuid = Guid.NewGuid();
        Name = name;
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Изменение имени тэга
    /// </summary>
    /// <param name="name">Новое имя тэга</param>
    public void SetName(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Изменение типа доступа к тэгу
    /// </summary>
    /// <param name="accessType">Новое имя тэга</param>
    public void SetAccessType(AccessType accessType)
    {
        AccessType = accessType;
    }

    public bool Equals(Tag? other)
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

        return obj is Tag tag && Equals(tag);
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
}