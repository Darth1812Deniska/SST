using System.Collections.Generic;
using SSTCommonClasses.Constructor.Models;

namespace SSTCommonClasses.ConstructorStorage.ObjectLists;

/// <summary>
/// Класс для доступа к списку sql-шаблонов в системе
/// </summary>
public class SqlTemplatesList : List<SqlTemplate>
{
    /// <summary>
    /// Добавление или обновление sql-шаблона
    /// </summary>
    /// <param name="sqlTemplate">Добавляемый или обновляемый sql-шаблон</param>
    public void AddOrReplace(SqlTemplate sqlTemplate)
    {
        bool isSqlTemplateExists = Exists(x => x.Equals(sqlTemplate));
        if (isSqlTemplateExists)
        {
            Remove(sqlTemplate);
        }
        Add(sqlTemplate);
    }

    /// <summary>
    /// Поиск sql-шаблона по уникальному идентификатору
    /// </summary>
    /// <param name="uuid">Уникальный идентификатор шаблона</param>
    /// <returns>Найденный шаблон либо null</returns>
    public SqlTemplate? GetById(Guid uuid)
    {
        return Find(x => x.Uuid == uuid);
    }
}