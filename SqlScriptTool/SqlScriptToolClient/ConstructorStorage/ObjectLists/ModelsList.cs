using System.Collections.Generic;
using SSTCommonClasses.Constructor.Models;

namespace SqlScriptToolClient.ConstructorStorage.ObjectLists;

public class ModelsList : List<Model>
{
    public void AddOrReplace(Model model)
    {
        bool isModelExists = Exists(x => x.Equals(model));
        if (isModelExists)
        {
            Remove(model);
        }
        Add(model);
    }
}