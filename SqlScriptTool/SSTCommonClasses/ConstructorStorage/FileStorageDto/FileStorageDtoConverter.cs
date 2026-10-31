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
        return dto;
    }

    /// <summary>
    /// Восстановление sql-шаблона из DTO (первый этап выгрузки).
    /// Текст скрипта подставляется отдельно (он хранится в файле .sql),
    /// ссылки на связанные объекты НЕ разрешаются — вместо этого сохраняются
    /// неразрешенные ссылки (Uuid) для второго этапа выгрузки, когда все DTO
    /// уже загружены в память.
    /// </summary>
    public static SqlTemplate FromDto(this SqlTemplateFileDto dto, string? sqlText)
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
            sqlTemplate.Tags.Add(tagDto.FromDto());
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
    /// для второго этапа выгрузки (<see cref="ResolveReferences"/>).
    /// </summary>
    public static Model FromDto(this ModelFileDto dto)
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
        model.RootModelItem = dto.RootModelItem.FromDto();
        return model;
    }

    /// <summary>
    /// Преобразование узла модели в DTO
    /// </summary>
    public static ModelItemFileDto ToDto(this ModelItem modelItem)
    {
        ModelItemFileDto dto = new()
        {
            Uuid = modelItem.Uuid,
            SqlTemplateUuid = modelItem.SqlTemplate?.Uuid ?? Guid.Empty,
            ChildModelItems = modelItem.ChildModelItems.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.ToDto())
        };
        return dto;
    }

    /// <summary>
    /// Восстановление узла модели из DTO (первый этап выгрузки).
    /// Ссылка на sql-шаблон сохраняется как неразрешенная (по Uuid) — она будет
    /// разрешена вторым этапом по уже загруженным в память объектам.
    /// </summary>
    public static ModelItem FromDto(this ModelItemFileDto dto)
    {
        ModelItem modelItem = ModelItem.CreateWithTemplateReference(dto.Uuid, dto.SqlTemplateUuid);

        foreach (KeyValuePair<string, ModelItemFileDto> childDto in dto.ChildModelItems)
        {
            modelItem.ChildModelItems.Add(childDto.Key, childDto.Value.FromDto());
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
            SqlTemplateUuid = tagTemplate.SqlTemplate?.Uuid
        };
        return dto;
    }

    /// <summary>
    /// Восстановление шаблона тэга из DTO. Ссылка на sql-шаблон разрешается через resolver.
    /// </summary>
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

        if (dto.SqlTemplateUuid is Guid sqlTemplateUuid
            && referenceResolver(sqlTemplateUuid) is SqlTemplate referencedTemplate)
        {
            tagTemplate.SqlTemplate = referencedTemplate;
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
    /// Восстановление тэга из DTO. Ссылки на связанные объекты разрешаются через resolver.
    /// </summary>
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

        if (dto.TagTemplateUuid is Guid tagTemplateUuid
            && referenceResolver(tagTemplateUuid) is TagTemplate referencedTagTemplate)
        {
            tag.TagTemplate = referencedTagTemplate;
        }

        if (dto.SqlTemplateUuid is Guid sqlTemplateUuid
            && referenceResolver(sqlTemplateUuid) is SqlTemplate referencedSqlTemplate)
        {
            tag.SqlTemplate = referencedSqlTemplate;
        }

        return tag;
    }

    #endregion
}
