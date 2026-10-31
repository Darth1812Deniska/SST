using System.Collections.Generic;
using SSTCommonClasses.Constructor.Models;

namespace SSTCommonClasses.ConstructorStorage.ObjectLists;

/// <summary>
/// Класс для доступа к списку моделей в системе
/// </summary>
public class ModelsList : List<Model>
{
    /// <summary>
    /// Добавление или обновление модели
    /// </summary>
    /// <param name="model">Добавляемая или обновляемая модель</param>
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