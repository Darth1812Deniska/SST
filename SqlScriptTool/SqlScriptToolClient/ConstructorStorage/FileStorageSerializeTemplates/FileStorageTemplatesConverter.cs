using SSTCommonClasses.Constants;
using SSTCommonClasses.Models;
using System.Collections.Generic;
using System;
using System.Linq;
using SSTCommonClasses.Tags;

namespace SqlScriptToolClient.ConstructorStorage.FileStorageSerializeTemplates;

public static class FileStorageTemplatesConverter
{
    #region public methods

    public static FileStorageModel GetFileStorageModel(Model model)
    {
        Guid uuid = model.Uuid;
        string name = model.Name;
        string displayLabel = model.DisplayLabel;
        string? description = model.Description;
        string creator = model.CreatorName;
        DbType dbType = model.DbType;
        AccessType accessType = model.AccessType;
        int version = model.Version;
        Guid rootModelItem = model.RootModelItem.Uuid;
        FileStorageModel result = new FileStorageModel(uuid, name, displayLabel, description, creator, dbType,
            accessType, version, rootModelItem);
        return result;
    }

    public static List<FileStorageModelItem> GetFileStorageModelItems(ModelItem rootModelItem)
    {
        List<FileStorageModelItem> result = new List<FileStorageModelItem>();
        result.AddRange(GetChildFileStorageModelItems(rootModelItem));
        return result;
    }

    public static FileStorageSqlTemplate GetFileStorageSqlTemplate(SqlTemplate sqlTemplate)
    {
        Guid uuid = sqlTemplate.Uuid;
        string name = sqlTemplate.Name;
        string displayLabel = sqlTemplate.DisplayLabel;
        string? description = sqlTemplate.Description;
        DbType dbType = sqlTemplate.DbType;
        int version = sqlTemplate.Version;
        AccessType accessType = sqlTemplate.AccessType;
        List<Guid> tags = sqlTemplate.Tags.Select(tag => tag.Uuid).ToList();
        FileStorageSqlTemplate result =
            new FileStorageSqlTemplate(uuid, name, displayLabel, description, dbType, version, accessType, tags);
        return result;
    }

    public static FileStorageTag GetFileStorageTag(Tag tag)
    {
        Guid uuid = tag.Uuid;
        string name = tag.Name;
        string? displayLabel = tag.DisplayLabel;
        string? description = tag.Description;
        TagType tagType = tag.TagType;
        TagDataType? dataType = tag.DataType;
        Guid? tagTemplate = tag.TagTemplate?.Uuid;
        Guid? ldObjectType = tag.LdObjectType?.SstGuid;
        Guid? childModelItem = tag.ChildModelItem?.Uuid;
        AccessType accessType = tag.AccessType;
        FileStorageTag result = new FileStorageTag(uuid, name, displayLabel, description, tagType, dataType,
            tagTemplate, ldObjectType, childModelItem, accessType);
        return result;
    }

    #endregion

    #region private methods

    private static List<FileStorageModelItem> GetChildFileStorageModelItems(ModelItem parentModelItem)
    {
        List<FileStorageModelItem> result = new List<FileStorageModelItem>();
        Guid uuid = parentModelItem.Uuid;
        Guid sqlTemplate = parentModelItem.SqlTemplate.Uuid;
        var childModelItems = parentModelItem.ChildModelItems
            .Select(dict => new KeyValuePair<Guid, Guid>(dict.Key.Uuid, dict.Value.Uuid))
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        FileStorageModelItem fileStorageModelItem = new FileStorageModelItem(uuid, sqlTemplate, childModelItems);
        result.Add(fileStorageModelItem);

        foreach (var modelItem in parentModelItem.ChildModelItems.Values)
        {
            result.AddRange(GetChildFileStorageModelItems(modelItem));
        }

        return result;
    }

    #endregion
}