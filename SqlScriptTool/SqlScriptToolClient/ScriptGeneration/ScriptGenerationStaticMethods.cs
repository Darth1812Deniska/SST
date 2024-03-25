using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SSTCommonClasses.Constructor.Models;

namespace SqlScriptToolClient.ScriptGeneration;

public static class ScriptGenerationStaticMethods
{
    public static string ReplaceParameters(string template, Dictionary<string, string> replacementValues)
    {
        string result = template;
        Regex rx = new Regex(@"<\w+>");
        result = rx.Replace(result, match =>
        {
            if (replacementValues.TryGetValue(match.Value, out var expression))
            {
                return expression;
            }

            return "NONE";
        }).Trim();
        return result;
    }

    public static string GetTemplateByModel(Model model)
    {
        var rootModelItem = model.RootModelItem;
        return GetModelItemSqlText(rootModelItem);
    }

    private static string GetModelItemSqlText(ModelItem modelItem)
    {
        var sqlTemplate = modelItem.SqlTemplate;
        if (sqlTemplate == null)
        {
            return string.Empty;
        }

        string templateText = modelItem.SqlTemplate.SqlText;

        if (string.IsNullOrEmpty(templateText))
        {
            return string.Empty;
        }

        Dictionary<string, ModelItem> childModelItems = modelItem.ChildModelItems;

        foreach (var childModelItem in childModelItems)
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