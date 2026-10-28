using System;
using System.Collections.Generic;

namespace SqlScriptToolAvalonia.ConstructorStorage.FileStorageSerializeTemplates;

[Serializable]
public class FileStorageModelItem
{
    public Guid? Uuid { get; set; }
    public Guid? SqlTemplate { get; set; }
    // <TagGuid, ModelItemGuid>
    public Dictionary<Guid, Guid>? ChildModelItems { get; set; }

    public FileStorageModelItem(Guid uuid, Guid sqlTemplate, Dictionary<Guid, Guid> childModelItems)
    {
        Uuid = uuid;
        SqlTemplate = sqlTemplate;
        ChildModelItems = childModelItems;
    }

    public FileStorageModelItem()
    {
    }
}