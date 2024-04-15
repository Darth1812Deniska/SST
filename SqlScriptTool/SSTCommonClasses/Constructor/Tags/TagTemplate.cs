using SSTCommonClasses.Constructor.Models;

namespace SSTCommonClasses.Constructor.Tags;

/// <summary>
/// Класс описывающий шаблоны тэгов
/// </summary>
[Serializable]
public class TagTemplate : ConstructorItem
{
    /// <summary>
    /// Тип шаблона тэга
    /// </summary>
    public TagType TagType { get; set; }
    /// <summary>
    /// Тип данных пользовательского параметра 
    /// </summary>
    public TagDataType? DataType { get; set; }
    /// <summary>
    /// SQL-шаблон привязанный к шаблону тэга
    /// </summary>
    public SqlTemplate? SqlTemplate { get; set; }

    /// <summary>
    /// Метка, указывающая, будет являться тэг, созданный на основе этого шаблона, родителем для множественного паттерна
    /// </summary>
    public bool IsMultiplePattern { get; set; }

    /// <summary>
    /// Создание экземпляра шаблона тэга
    /// </summary>
    public TagTemplate()
    {
    }
    
    public bool Equals(TagTemplate? other)
    {
        if (other == null)
            return false;
        return Uuid == other.Uuid;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }

        return obj is TagTemplate tagTemplate && Equals(tagTemplate);
    }

    public override string ToString()
    {
        return $"{Name} ({DisplayLabel})";
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Uuid);
    }
}