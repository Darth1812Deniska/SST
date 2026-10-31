using System;
using System.Collections.Generic;
using SSTCommonClasses.Constants;
using SSTCommonClasses.Constructor.Tags;

namespace SSTCommonClasses.ConstructorStorage.FileStorageDto;

/// <summary>
/// DTO для сериализации sql-шаблона в файловую систему.
/// Содержит только атрибуты объекта: текст скрипта сохраняется в отдельный файл .sql,
/// а связанные объекты (тэги) сериализуются в виде ссылок по уникальному идентификатору.
/// </summary>
public class SqlTemplateFileDto
{
    /// <summary>
    /// Уникальный идентификатор sql-шаблона
    /// </summary>
    public Guid Uuid { get; set; }

    /// <summary>
    /// Название шаблона
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Описание шаблона
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Отображаемое название шаблона
    /// </summary>
    public string? DisplayLabel { get; set; }

    /// <summary>
    /// Тип доступа к шаблону
    /// </summary>
    public AccessType AccessType { get; set; }

    /// <summary>
    /// Создатель шаблона
    /// </summary>
    public string? Creator { get; set; }

    /// <summary>
    /// Тип СУБД, для которой разрабатывается шаблон
    /// </summary>
    public DbType DbType { get; set; }

    /// <summary>
    /// Номер версии шаблона
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Метка множественного паттерна
    /// </summary>
    public bool IsMultiplePattern { get; set; }

    /// <summary>
    /// Ссылки на тэги шаблона (вместо объектов целиком — уникальные идентификаторы соответствующих объектов)
    /// </summary>
    public List<TagFileDto> Tags { get; set; } = new();
}

/// <summary>
/// DTO для сериализации модели в файловую систему.
/// Текст связанных sql-шаблонов и вложенные объекты сохраняются отдельно,
/// в дереве узлов вместо ссылок на объект целиком прописываются уникальные идентификаторы.
/// </summary>
public class ModelFileDto
{
    /// <summary>
    /// Уникальный идентификатор модели
    /// </summary>
    public Guid Uuid { get; set; }

    /// <summary>
    /// Название модели
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Описание модели
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Отображаемое название модели
    /// </summary>
    public string? DisplayLabel { get; set; }

    /// <summary>
    /// Тип доступа к модели
    /// </summary>
    public AccessType AccessType { get; set; }

    /// <summary>
    /// Тип СУБД, для которой разрабатывается модель
    /// </summary>
    public DbType DbType { get; set; }

    /// <summary>
    /// Разработчик модели
    /// </summary>
    public string? CreatorName { get; set; }

    /// <summary>
    /// Номер версии модели
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Корневой узел модели (ссылки на sql-шаблоны — по уникальным идентификаторам)
    /// </summary>
    public ModelItemFileDto RootModelItem { get; set; } = new();
}

/// <summary>
/// DTO для сериализации узла модели в файловую систему
/// </summary>
public class ModelItemFileDto
{
    /// <summary>
    /// Уникальный идентификатор узла
    /// </summary>
    public Guid Uuid { get; set; }

    /// <summary>
    /// Ссылка на прикрепленный sql-шаблон (уникальный идентификатор соответствующего объекта)
    /// </summary>
    public Guid SqlTemplateUuid { get; set; }

    /// <summary>
    /// Дочерние узлы
    /// </summary>
    public Dictionary<string, ModelItemFileDto> ChildModelItems { get; set; } = new();
}

/// <summary>
/// DTO для сериализации шаблона тэга в файловую систему.
/// Связанный sql-шаблон сохраняется как ссылка по уникальному идентификатору.
/// </summary>
public class TagTemplateFileDto
{
    /// <summary>
    /// Уникальный идентификатор шаблона тэга
    /// </summary>
    public Guid Uuid { get; set; }

    /// <summary>
    /// Название шаблона тэга
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Описание шаблона тэга
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Отображаемое название шаблона тэга
    /// </summary>
    public string? DisplayLabel { get; set; }

    /// <summary>
    /// Тип доступа к шаблону тэга
    /// </summary>
    public AccessType AccessType { get; set; }

    /// <summary>
    /// Тип шаблона тэга
    /// </summary>
    public TagType TagType { get; set; }

    /// <summary>
    /// Тип данных пользовательского параметра
    /// </summary>
    public TagDataType? DataType { get; set; }

    /// <summary>
    /// Метка множественного паттерна
    /// </summary>
    public bool IsMultiplePattern { get; set; }

    /// <summary>
    /// Ссылка на привязанный sql-шаблон (уникальный идентификатор соответствующего объекта).
    /// null — если sql-шаблон не привязан.
    /// </summary>
    public Guid? SqlTemplateUuid { get; set; }
}

/// <summary>
/// DTO для сериализации тэга (внутри файла атрибутов sql-шаблона).
/// Вместо ссылок на объекты целиком прописываются уникальные идентификаторы соответствующих объектов.
/// </summary>
public class TagFileDto
{
    /// <summary>
    /// Уникальный идентификатор тэга
    /// </summary>
    public Guid Uuid { get; set; }

    /// <summary>
    /// Название тэга
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Описание тэга
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Отображаемое название тэга
    /// </summary>
    public string? DisplayLabel { get; set; }

    /// <summary>
    /// Тип доступа к тэгу
    /// </summary>
    public AccessType AccessType { get; set; }

    /// <summary>
    /// Тип тэга
    /// </summary>
    public TagType? TagType { get; set; }

    /// <summary>
    /// Тип данных тэга пользовательских параметров
    /// </summary>
    public TagDataType? DataType { get; set; }

    /// <summary>
    /// Ссылка на шаблон тэга (уникальный идентификатор соответствующего объекта).
    /// null — если тэг создан без шаблона.
    /// </summary>
    public Guid? TagTemplateUuid { get; set; }

    /// <summary>
    /// Ссылка на дочерний sql-шаблон (уникальный идентификатор соответствующего объекта).
    /// null — если дочерний шаблон отсутствует.
    /// </summary>
    public Guid? SqlTemplateUuid { get; set; }

    /// <summary>
    /// Метка множественного паттерна
    /// </summary>
    public bool? IsMultiplePattern { get; set; }

    /// <summary>
    /// Порядковый номер тэга в шаблоне
    /// </summary>
    public int? Order { get; set; }

    /// <summary>
    /// Значение по умолчанию для тэга. Может отсутствовать
    /// </summary>
    public object? DefaultValue { get; set; }
}
