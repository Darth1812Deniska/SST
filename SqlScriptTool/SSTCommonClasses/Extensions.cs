using System;
using System.Collections.Generic;

namespace SSTCommonClasses
{
    public static class Extensions
    {
        public static string FirstLetterToUpperCase(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        //TODO: remove after Avalonia migration (duplicate of SqlScriptToolAvalonia.Extensions.CollectionExtensions)
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
                collection.Add(item);
        }
    }
}
