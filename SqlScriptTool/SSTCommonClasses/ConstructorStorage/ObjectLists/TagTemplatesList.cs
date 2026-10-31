using System.Collections.Generic;
using SSTCommonClasses.Constructor.Tags;

namespace SSTCommonClasses.ConstructorStorage.ObjectLists;

/// <summary>
/// Класс для доступа к списку шаблонов тэгов в системе
/// </summary>
public class TagTemplatesList:List<TagTemplate>
{
    /// <summary>
    /// Добавление или обновление шаблона тэга
    /// </summary>
    /// <param name="tagTemplate">Добавляемый или обновляемый шаблон тэга</param>
    public void AddOrReplace(TagTemplate tagTemplate)
    {
        bool isTagExists = Exists(x => x.Equals(tagTemplate));
        if (isTagExists)
        {
            Remove(tagTemplate);
        }
        Add(tagTemplate);
    }

    /// <summary>
    /// Поиск шаблона тэга по уникальному идентификатору
    /// </summary>
    /// <param name="uuid">Уникальный идентификатор шаблона тэга</param>
    /// <returns>Найденный шаблон тэга либо null</returns>
    public TagTemplate? GetById(Guid uuid)
    {
        return Find(x => x.Uuid == uuid);
    }
}