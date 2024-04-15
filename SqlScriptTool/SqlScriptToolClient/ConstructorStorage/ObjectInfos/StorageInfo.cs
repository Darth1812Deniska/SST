using SSTCommonClasses.Constants;

namespace SqlScriptToolClient.ConstructorStorage.ObjectInfos;

/// <summary>
/// Класс для хранения информации о директориях клиентской части в файловой системе
/// </summary>
public class StorageInfo
{
    /// <summary>
    /// Тип Доступа элемента конструктора SST 
    /// </summary>
    public AccessType AccessType { get; set; }
    /// <summary>
    /// Тип элемента конструктора SST
    /// </summary>
    public ConstructorType ConstructorObjectTypes { get; set; }
    /// <summary>
    /// Путь к директории хранения элементов с заданным типом доступа и указанным типом элемента конструктора
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// Создание объекта с информацией о директориях клиентской части в файловой системе
    /// </summary>
    /// <param name="accessType">Тип Доступа</param>
    /// <param name="constructorObjectTypes">Тип элемента конструктора SST</param>
    /// <param name="path">Путь к директории хранения элементов</param>
    public StorageInfo(AccessType accessType, ConstructorType constructorObjectTypes, string path)
    {
        AccessType = accessType;
        ConstructorObjectTypes = constructorObjectTypes;
        Path = path;
    }
}