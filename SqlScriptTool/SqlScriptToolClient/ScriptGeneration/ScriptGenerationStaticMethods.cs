using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SqlScriptToolClient.ScriptGeneration;

public static class ScriptGenerationStaticMethods
{
    public static string ReplaceParameters(string template, Dictionary<string, string> replacementValues)
    {
        string result = template;
        Regex rx = new Regex(@"<\w+>");
        result = rx.Replace(result, (match) =>
        {
            if (replacementValues.TryGetValue(match.Value, out var expression))
            {
                return expression;
            }

            return "NONE";
        }).Trim();
        return result;
    }
}