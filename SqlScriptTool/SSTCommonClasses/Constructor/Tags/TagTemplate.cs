using SSTCommonClasses.Constructor.Models;

namespace SSTCommonClasses.Constructor.Tags;

/// <summary>
/// Класс описывающий шаблоны тэгов
/// </summary>
[Serializable]
public class TagTemplate : ConstructorItem
{
    /// <summary>
    /// Тип шаблона тэга
    /// </summary>
    public TagType TagType { get; set; }
    /// <summary>
    /// Тип данных пользовательского параметра 
    /// </summary>
    public TagDataType? DataType { get; set; }
    /// <summary>
    /// SQL-шаблон привязанный к шаблону тэга.
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

    private SqlTemplate? _sqlTemplate;
    private Guid? _savedSqlTemplateUuid;

    /// <summary>
    /// Уникальный идентификатор sql-шаблона, на который ссылается шаблон тэга
    /// (в том числе неразрешенная ссылка, сохраненная при первом этапе выгрузки).
    /// null — если ссылка отсутствовала в сохраненных данных.
    /// </summary>
    public Guid? SqlTemplateUuid => _sqlTemplate?.Uuid ?? _savedSqlTemplateUuid;

    /// <summary>
    /// Установка ссылки шаблона тэга на sql-шаблон по уникальному идентификатору
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
    /// Метка, указывающая, будет являться тэг, созданный на основе этого шаблона, родителем для множественного паттерна
    /// </summary>
    public bool IsMultiplePattern { get; set; }

    /// <summary>
    /// Создание экземпляра шаблона тэга
    /// </summary>
    public TagTemplate()
    {
    }
    
    public bool Equals(TagTemplate? other)
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

        return obj is TagTemplate tagTemplate && Equals(tagTemplate);
    }

    public override string ToString()
    {
        return $"{Name} ({DisplayLabel})";
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Uuid);
    }
}