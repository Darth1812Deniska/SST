using System.Collections.Generic;
using SSTCommonClasses.Constructor.Tags;

namespace SqlScriptToolClient.ConstructorStorage.ObjectLists;

public class TagTemplatesList:List<TagTemplate>
{
    public void AddOrReplace(TagTemplate tagTemplate)
    {
        bool isTagExists = Exists(x => x.Equals(tagTemplate));
        if (isTagExists)
        {
            Remove(tagTemplate);
        }
        Add(tagTemplate);
    }
}