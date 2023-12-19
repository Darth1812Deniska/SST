using System.Collections.Generic;
using SSTCommonClasses.Models;

namespace SqlScriptToolClient.ConstructorStorage.ObjectLists;

public class SqlTemplatesList : List<SqlTemplate>
{
    public void AddOrReplace(SqlTemplate sqlTemplate)
    {
        bool isSqlTemplateExists = Exists(x => x.Equals(sqlTemplate));
        if (isSqlTemplateExists)
        {
            Remove(sqlTemplate);
        }
        Add(sqlTemplate);
    }
}