using System.Collections.Generic;
using System.Text.RegularExpressions;
using SSTCommonClasses.Constructor.Models;

namespace SSTCommonClasses.ScriptGeneration;

/// <summary>
/// Статический класс предоставляющий методы по обработке шаблонов скриптов
/// </summary>
public static class ScriptGenerationStaticMethods
{
    /// <summary>
    /// Метод подставляющий значения в шаблон скрипта
    /// </summary>
    /// <param name="template">Текст шаблона скрипта</param>
    /// <param name="replacementValues">Список параметров с их значениями</param>
    /// <returns>Итоговый скрипт</returns>
    public static string ReplaceParameters(string template, Dictionary<string, string> replacementValues)
    {
        string result = template;
        Regex rx = new Regex(@"<\w+>");
        result = rx.Replace(result, match =>
        {
            if (replacementValues.TryGetValue(match.Value, out string? expression))
            {
                return expression;
            }

            return "NONE";
        }).Trim();
        return result;
    }

    /// <summary>
    /// Метод формирующий текст шаблона скрипта на основе переданной модели
    /// </summary>
    /// <param name="model">Модель, на основе которой формируется шаблон скрипта</param>
    /// <returns>Текст шаблона скрипта</returns>
    public static string GetTemplateByModel(Model model)
    {
        ModelItem rootModelItem = model.RootModelItem;
        return GetModelItemSqlText(rootModelItem);
    }

    /// <summary>
    /// Рекурсивный метод, собирвющий текст шаблона скрипта для указанного узла модели
    /// </summary>
    /// <param name="modelItem">Узел модели, из которого получается текст шаблона скрипта</param>
    /// <returns>Сформированный текст шаблона скрипта для заданного узла</returns>
    private static string GetModelItemSqlText(ModelItem modelItem)
    {
        SqlTemplate sqlTemplate = modelItem.SqlTemplate;

        if (sqlTemplate.IsMultiplePattern)
        {
            return string.Empty;
        }

        string? templateText = modelItem.SqlTemplate.SqlText;

        if (string.IsNullOrEmpty(templateText))
        {
            return string.Empty;
        }

        Dictionary<string, ModelItem> childModelItems = modelItem.ChildModelItems;

        foreach (KeyValuePair<string, ModelItem> childModelItem in childModelItems)
        {
            string tagText = childModelItem.Key;
            string childSqlText = GetModelItemSqlText(childModelItem.Value);

            if (string.IsNullOrEmpty(childSqlText))
            {
                continue;
            }

            templateText = templateText.Replace(tagText, childSqlText);
        }

        return templateText;
    }
}