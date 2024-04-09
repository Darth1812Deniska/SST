using System;
using SSTCommonClasses.Constants;


namespace SqlScriptToolClient.ConstructorStorage.ObjectInfos;

/// <summary>
/// Класс для описания информации о хранении моделей в файловой системе
/// </summary>
public class ModelInfo
{
    /// <summary>
    /// Уникальный идентификатор модели
    /// </summary>
    public Guid? ModelUuid { get; set; }
    /// <summary>
    /// Наименование модели
    /// </summary>
    public string? ModelName { get; set; }
    /// <summary>
    /// Тип доступа к модели
    /// </summary>
    public AccessType? ModelAccessType { get; set; }
    /// <summary>
    /// Путь к файлу с моделью
    /// </summary>
    public string? ModelPath { get; set; }
    /// <summary>
    /// Название файла модели
    /// </summary>
    public string? ModelFileName { get; set; }
    //public string ModelItemsFileName { get; set; }

    public ModelInfo()
    {
    }

    public bool Equals(ModelInfo? other)
    {
        if (other == null)
            return false;
        return ModelUuid == other.ModelUuid;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }

        return obj is ModelInfo model && Equals(model);
    }

    public override int GetHashCode()
    {
        return ModelUuid.GetHashCode();
    }
}