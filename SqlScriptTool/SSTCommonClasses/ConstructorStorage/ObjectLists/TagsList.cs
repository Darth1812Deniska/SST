using System.Collections.Generic;
using SSTCommonClasses.Constructor.Tags;

namespace SSTCommonClasses.ConstructorStorage.ObjectLists;

/// <summary>
/// Класс для доступа к списку тэгов
/// </summary>
public class TagsList : List<Tag>
{
    /// <summary>
    /// Добавление или обновление тэга
    /// </summary>
    /// <param name="tag">Добавляемый или обновляемый тэг</param>
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