using System.Collections.Generic;
using SSTCommonClasses.Tags;

namespace SqlScriptToolClient.ConstructorStorage.ObjectLists;

public class TagsList : List<Tag>
{
    public void AddOrReplace(Tag tag)
    {
        bool isTagExists = Exists(x => x.Equals(tag));
        if (isTagExists)
        {
            Remove(tag);
        }
        Add(tag);
    }
}