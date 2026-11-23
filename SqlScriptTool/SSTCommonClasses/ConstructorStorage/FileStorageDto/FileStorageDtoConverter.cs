using System;
using System.Collections.Generic;
using System.Linq;
using SSTCommonClasses.Constructor.Models;
using SSTCommonClasses.Constructor.Tags;

namespace SSTCommonClasses.ConstructorStorage.FileStorageDto;

/// <summary>
/// Класс-конвертер между доменными объектами конструктора и DTO для хранения в файловой системе.
/// При конвертации в DTO связанные объекты (sql-шаблоны, шаблоны тэгов) заменяются
/// на уникальные идентификаторы соответствующих объектов, чтобы не сериализовать
/// объекты целиком со всеми вложениями.
/// </summary>
public static class FileStorageDtoConverter
{
    #region SqlTemplate

    /// <summary>
    /// Преобразование sql-шаблона в DTO (без текста скрипта и без вложенных объектов — только атрибуты и ссылки по Uuid)
    /// </summary>
    public static SqlTemplateFileDto ToDto(this SqlTemplate sqlTemplate)
    {
        SqlTemplateFileDto dto = new()
        {
            Uuid = sqlTemplate.Uuid,
            Name = sqlTemplate.Name,
            Description = sqlTemplate.Description,
            DisplayLabel = sqlTemplate.DisplayLabel,
            AccessType = sqlTemplate.AccessType,
            Creator = sqlTemplate.Creator,
            DbType = sqlTemplate.DbType,
            Version = sqlTemplate.Version,
            IsMultiplePattern = sqlTemplate.IsMultiplePattern,
            Tags = sqlTemplate.Tags.Select(tag => tag.ToDto()).ToList()
        };

        // Запоминаем неразрешенные ссылки тэгов (Uuid связанных объектов), чтобы второй
        // этап выгрузки мог их разрешить даже если объекты ещё не загружены в память
        foreach (Tag tag in sqlTemplate.Tags)
        {
            if (tag.TagTemplate == null && tag.TagTemplateUuid is Guid unresolvedTagTemplateUuid)
            {
                dto.UnresolvedReferences[unresolvedTagTemplateUuid] = nameof(TagFileDto.TagTemplateUuid);
            }

            if (tag.AttachedSqlTemplate == null && tag.SqlTemplateUuid is Guid unresolvedSqlTemplateUuid)
            {
                dto.UnresolvedReferences[unresolvedSqlTemplateUuid] = nameof(TagFileDto.SqlTemplateUuid);
            }
        }

        return dto;
    }

    /// <summary>
    /// Восстановление sql-шаблона из DTO (первый этап выгрузки).
    /// Текст скрипта подставляется отдельно (он хранится в файле .sql),
    /// ссылки на связанные объекты НЕ разрешаются — resolver на первом этапе
    /// возвращает null, а неразрешенные ссылки (Uuid) сохраняются в объектах тэгов
    /// для второго этапа выгрузки (<see cref="StaticConstructorStorage.ResolveLoadedReferences"/>),
    /// когда все DTO уже загружены в память.
    /// </summary>
    /// <param name="dto">DTO sql-шаблона, прочитанный из файла атрибутов .json</param>
    /// <param name="referenceResolver">Функция разрешения ссылок по уникальному идентификатору
    /// (на первом этапе передаётся пустой resolver, возвращающий null)</param>
    /// <param name="sqlText">Текст sql-скрипта, прочитанный из файла .sql в той же папке</param>
    public static SqlTemplate FromDto(this SqlTemplateFileDto dto, Func<Guid, object?> referenceResolver,
        string? sqlText)
    {
        SqlTemplate sqlTemplate = new()
        {
            Uuid = dto.Uuid,
            Name = dto.Name,
            Description = dto.Description,
            DisplayLabel = dto.DisplayLabel,
            AccessType = dto.AccessType,
            Creator = dto.Creator,
            DbType = dto.DbType,
            Version = dto.Version,
            IsMultiplePattern = dto.IsMultiplePattern,
            SqlText = sqlText
        };

        foreach (TagFileDto tagDto in dto.Tags)
        {
            Tag tag = tagDto.FromDto(referenceResolver);
            // Если ссылка не разрешилась (первый этап), сохраняем Uuid в самом тэге,
            // чтобы второй этап мог разрешить её по уже загруженным в память объектам.
            // Дополнительно проверяем неразрешенные ссылки самого DTO шаблона — они
            // заполняются при сохранении объекта с ещё не разрешенными зависимостями.
            if (tag.TagTemplate == null && tagDto.TagTemplateUuid is Guid unresolvedTagTemplateUuid)
            {
                tag.SetTagTemplateReference(unresolvedTagTemplateUuid);
            }

            if (tag.AttachedSqlTemplate == null && tagDto.SqlTemplateUuid is Guid unresolvedSqlTemplateUuid)
            {
                tag.SetSqlTemplateReference(unresolvedSqlTemplateUuid);
            }

            sqlTemplate.Tags.Add(tag);
        }

        return sqlTemplate;
    }

    #endregion

    #region Model

    /// <summary>
    /// Преобразование модели в DTO (узлы сохраняются с ссылками на sql-шаблоны по Uuid)
    /// </summary>
    public static ModelFileDto ToDto(this Model model)
    {
        ModelFileDto dto = new()
        {
            Uuid = model.Uuid,
            Name = model.Name,
            Description = model.Description,
            DisplayLabel = model.DisplayLabel,
            AccessType = model.AccessType,
            DbType = model.DbType,
            CreatorName = model.CreatorName,
            Version = model.Version,
            RootModelItem = model.RootModelItem.ToDto()
        };
        return dto;
    }

    /// <summary>
    /// Восстановление модели из DTO (первый этап выгрузки).
    /// Ссылки на sql-шаблоны не разрешаются — неразрешенные ссылки сохраняются
    /// в объектах узлов (по Uuid) для второго этапа выгрузки
    /// (<see cref="StaticConstructorStorage.ResolveLoadedReferences"/>).
    /// </summary>
    /// <param name="dto">DTO модели, прочитанный из файла атрибутов .json</param>
    /// <param name="referenceResolver">Функция разрешения ссылок по уникальному идентификатору
    /// (на первом этапе передаётся пустой resolver, возвращающий null)</param>
    /// <param name="storageRootPath">Зарезервированный параметр: корневой путь хранилища
    /// (может использоваться для загрузки вложенных файлов, относящихся к модели)</param>
    public static Model FromDto(this ModelFileDto dto, Func<Guid, object?> referenceResolver,
        string? storageRootPath)
    {
        Model model = new()
        {
            Uuid = dto.Uuid,
            Name = dto.Name,
            Description = dto.Description,
            DisplayLabel = dto.DisplayLabel,
            AccessType = dto.AccessType,
            DbType = dto.DbType,
            CreatorName = dto.CreatorName,
            Version = dto.Version
        };
        // Корневой узел восстанавливается из DTO вместе с деревом дочерних узлов
        // (без ленивого создания пустого шаблона, который подменял бы сохраненную ссылку)
        model.RootModelItem = dto.RootModelItem.FromDto(referenceResolver);
        return model;
    }

    /// <summary>
    /// Преобразование узла модели в DTO. Узел хранит только ссылку на sql-шаблон
    /// (<see cref="ModelItem.SqlTemplateUuid"/>), поэтому в DTO записывается именно она,
    /// без сериализации самого шаблона (он выгружается в хранилище отдельно).
    /// </summary>
    public static ModelItemFileDto ToDto(this ModelItem modelItem)
    {
        ModelItemFileDto dto = new()
        {
            Uuid = modelItem.Uuid,
            SqlTemplateUuid = modelItem.SqlTemplateUuid,
            ChildModelItems = modelItem.ChildModelItems.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.ToDto())
        };

        // Если шаблон по ссылке ещё не выгружен в систему — запоминаем неразрешенную ссылку,
        // чтобы второй этап выгрузки смог её разрешить
        if (modelItem.SqlTemplateUuid != Guid.Empty && ModelItem.GetSqlTemplate(modelItem.SqlTemplateUuid) == null)
        {
            dto.UnresolvedReferences[modelItem.SqlTemplateUuid] = nameof(ModelItemFileDto.SqlTemplateUuid);
        }

        return dto;
    }

    /// <summary>
    /// Восстановление узла модели из DTO (первый этап выгрузки).
    /// Ссылка на sql-шаблон сохраняется как неразрешенная (по Uuid) — она будет
    /// разрешена вторым этапом по уже загруженным в память объектам.
    /// </summary>
    /// <param name="dto">DTO узла модели</param>
    /// <param name="referenceResolver">Функция разрешения ссылок по уникальному идентификатору
    /// (на первом этапе передаётся пустой resolver, возвращающий null)</param>
    public static ModelItem FromDto(this ModelItemFileDto dto, Func<Guid, object?> referenceResolver)
    {
        // Узел создаётся сразу со ссылкой на сохраненный sql-шаблон; сам шаблон
        // получается из хранилища методом ModelItem.GetSqlTemplate() по необходимости
        ModelItem modelItem = ModelItem.CreateWithTemplateReference(dto.Uuid, dto.SqlTemplateUuid);

        foreach (KeyValuePair<string, ModelItemFileDto> childDto in dto.ChildModelItems)
        {
            modelItem.ChildModelItems.Add(childDto.Key, childDto.Value.FromDto(referenceResolver));
        }

        return modelItem;
    }

    #endregion

    #region TagTemplate

    /// <summary>
    /// Преобразование шаблона тэга в DTO (связанный sql-шаблон — только ссылка по Uuid)
    /// </summary>
    public static TagTemplateFileDto ToDto(this TagTemplate tagTemplate)
    {
        TagTemplateFileDto dto = new()
        {
            Uuid = tagTemplate.Uuid,
            Name = tagTemplate.Name,
            Description = tagTemplate.Description,
            DisplayLabel = tagTemplate.DisplayLabel,
            AccessType = tagTemplate.AccessType,
            TagType = tagTemplate.TagType,
            DataType = tagTemplate.DataType,
            IsMultiplePattern = tagTemplate.IsMultiplePattern,
            SqlTemplateUuid = tagTemplate.SqlTemplateUuid
        };

        // Если шаблон по ссылке ещё не выгружен в систему — запоминаем неразрешенную ссылку
        if (tagTemplate.SqlTemplate == null && tagTemplate.SqlTemplateUuid is Guid unresolvedSqlTemplateUuid)
        {
            dto.UnresolvedReferences[unresolvedSqlTemplateUuid] = nameof(TagTemplateFileDto.SqlTemplateUuid);
        }

        return dto;
    }

    /// <summary>
    /// Восстановление шаблона тэга из DTO (первый этап выгрузки).
    /// Ссылка на sql-шаблон не разрешается — неразрешенная ссылка (Uuid) сохраняется
    /// в объекте для второго этапа выгрузки (<see cref="StaticConstructorStorage.ResolveLoadedReferences"/>).
    /// </summary>
    /// <param name="dto">DTO шаблона тэга, прочитанный из файла атрибутов .json</param>
    /// <param name="referenceResolver">Функция разрешения ссылок по уникальному идентификатору
    /// (на первом этапе передаётся пустой resolver, возвращающий null)</param>
    public static TagTemplate FromDto(this TagTemplateFileDto dto, Func<Guid, object?> referenceResolver)
    {
        TagTemplate tagTemplate = new()
        {
            Uuid = dto.Uuid,
            Name = dto.Name,
            Description = dto.Description,
            DisplayLabel = dto.DisplayLabel,
            AccessType = dto.AccessType,
            TagType = dto.TagType,
            DataType = dto.DataType,
            IsMultiplePattern = dto.IsMultiplePattern
        };

        if (dto.SqlTemplateUuid is Guid sqlTemplateUuid)
        {
            if (referenceResolver(sqlTemplateUuid) is SqlTemplate referencedTemplate)
            {
                tagTemplate.SqlTemplate = referencedTemplate;
            }
            else
            {
                // Первый этап: сохраняем неразрешенную ссылку для второго этапа
                tagTemplate.SetSqlTemplateReference(sqlTemplateUuid);
            }
        }

        return tagTemplate;
    }

    #endregion

    #region Tag

    /// <summary>
    /// Преобразование тэга в DTO (связанные объекты — только ссылки по Uuid)
    /// </summary>
    public static TagFileDto ToDto(this Tag tag)
    {
        TagFileDto dto = new()
        {
            Uuid = tag.Uuid,
            Name = tag.Name,
            Description = tag.Description,
            DisplayLabel = tag.DisplayLabel,
            AccessType = tag.AccessType,
            TagType = tag.TagType,
            DataType = tag.DataType,
            TagTemplateUuid = tag.TagTemplate?.Uuid,
            SqlTemplateUuid = tag.SqlTemplate?.Uuid,
            IsMultiplePattern = tag.IsMultiplePattern,
            Order = tag.Order,
            DefaultValue = tag.DefaultValue
        };
        return dto;
    }

    /// <summary>
    /// Восстановление тэга из DTO (первый этап выгрузки).
    /// Ссылки на связанные объекты не разрешаются — неразрешенные ссылки (Uuid) сохраняются
    /// в объекте тэга для второго этапа выгрузки
    /// (<see cref="StaticConstructorStorage.ResolveLoadedReferences"/>).
    /// </summary>
    /// <param name="dto">DTO тэга из файла атрибутов sql-шаблона</param>
    /// <param name="referenceResolver">Функция разрешения ссылок по уникальному идентификатору
    /// (на первом этапе передаётся пустой resolver, возвращающий null)</param>
    public static Tag FromDto(this TagFileDto dto, Func<Guid, object?> referenceResolver)
    {
        Tag tag = new()
        {
            Uuid = dto.Uuid,
            Name = dto.Name,
            Description = dto.Description,
            DisplayLabel = dto.DisplayLabel,
            AccessType = dto.AccessType,
            TagType = dto.TagType,
            DataType = dto.DataType,
            IsMultiplePattern = dto.IsMultiplePattern,
            Order = dto.Order,
            DefaultValue = dto.DefaultValue
        };

        if (dto.TagTemplateUuid is Guid tagTemplateUuid)
        {
            if (referenceResolver(tagTemplateUuid) is TagTemplate referencedTagTemplate)
            {
                tag.TagTemplate = referencedTagTemplate;
            }
            else
            {
                // Первый этап: сохраняем неразрешенную ссылку для второго этапа
                tag.SetTagTemplateReference(tagTemplateUuid);
            }
        }

        if (dto.SqlTemplateUuid is Guid sqlTemplateUuid)
        {
            if (referenceResolver(sqlTemplateUuid) is SqlTemplate referencedSqlTemplate)
            {
                tag.SqlTemplate = referencedSqlTemplate;
            }
            else
            {
                // Первый этап: сохраняем неразрешенную ссылку для второго этапа
                tag.SetSqlTemplateReference(sqlTemplateUuid);
            }
        }

        return tag;
    }

    #endregion
}
