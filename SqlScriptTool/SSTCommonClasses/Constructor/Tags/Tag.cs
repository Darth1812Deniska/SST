using SSTCommonClasses.Constants;
using SSTCommonClasses.Constructor.Models;

namespace SSTCommonClasses.Constructor.Tags;

/// <summary>
/// Класс описывающий тэги sql-шаблонов
/// </summary>
[Serializable]
public class Tag : ConstructorItem
{
    #region Properties
    /// <summary>
    /// Тип тэга
    /// </summary>
    public TagType? TagType { get; set; }
    /// <summary>
    /// Тип данных тэга пользовательских параметров
    /// </summary>
    public TagDataType? DataType { get; set; }
    /// <summary>
    /// Шаблон тэга, на основе которого создан текущий тэг
    /// </summary>
    public TagTemplate? TagTemplate { get; set; }
    /// <summary>
    /// Дочерний sql-щаблон привязанный к тэгу
    /// </summary>
    public SqlTemplate? SqlTemplate { get; set; }
    /// <summary>
    /// Метка, указывающая, является ли тэг родителем для множественного паттерна
    /// </summary>
    public bool? IsMultiplePattern { get; set; }
    
    /// <summary>
    /// Порядковый номер тега в шаблне
    /// </summary>
    /// 
    public int? Order { get; set; }
    
    /// <summary>
    /// Значение по умолчанию для тега. Может отсутствовать 
    /// </summary>
    public object? DefaultValue { get; set; }

    #endregion


    #region Constructors

    /// <summary>
    /// Создание нового экземпляра тэга
    /// </summary>
    public Tag()
    {
    }

    /// <summary>
    /// Создание нового экземпляра тэга с заданным именем
    /// </summary>
    /// <param name="name">Имя тэга</param>
    public Tag(string name)
    {
        Uuid = Guid.NewGuid();
        Name = name;
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Изменение имени тэга
    /// </summary>
    /// <param name="name">Новое имя тэга</param>
    public void SetName(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Изменение типа доступа к тэгу
    /// </summary>
    /// <param name="accessType">Новое имя тэга</param>
    public void SetAccessType(AccessType accessType)
    {
        AccessType = accessType;
    }

    public bool Equals(Tag? other)
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

        return obj is Tag tag && Equals(tag);
    }

    public override string ToString()
    {
        return $"{Name} ({DisplayLabel})";
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Uuid);
    }

    #endregion
}