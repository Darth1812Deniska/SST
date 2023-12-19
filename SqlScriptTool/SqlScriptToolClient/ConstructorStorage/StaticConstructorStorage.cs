using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SqlScriptToolClient.ConstructorStorage.ObjectInfos;
using SqlScriptToolClient.ConstructorStorage.ObjectLists;
using SSTCommonClasses.Constants;
using SSTCommonClasses.Models;
using SSTCommonClasses.Tags;

namespace SqlScriptToolClient.ConstructorStorage;

public static class StaticConstructorStorage
{
    #region fields

    private static ModelsList _models = new ModelsList();
    private static SqlTemplatesList _sqlTemplates = new SqlTemplatesList();
    private static TagsList _tags = new TagsList();
    private static List<StorageInfo> _storageInfos = new List<StorageInfo>();

    #endregion

    #region properties

    public static ModelsList Models => _models;
    public static SqlTemplatesList SqlTemplates => _sqlTemplates;
    public static TagsList Tags => _tags;
    public static List<StorageInfo> StorageInfos => _storageInfos;
    public static string? UserDataRootPath { get; set; }

    #endregion

    #region public methods

    public static void LoadModels()
    {
        _models = new ModelsList();
    }

    public static void LoadSqlTemplates()
    {
        _sqlTemplates = new SqlTemplatesList();
    }

    public static void LoadTags()
    {
        _tags = new TagsList();
    }

    public static void LoadStorage()
    {
        LoadStorageInfos();
        LoadModels();
        LoadSqlTemplates();
        LoadTags();
    }

    public static void LoadStorageInfos()
    {
        if (string.IsNullOrEmpty(UserDataRootPath))
        {
            return;
        }

        Directory.SetCurrentDirectory(UserDataRootPath);
        if (File.Exists("StorageInfos.json"))
        {
            string json = File.ReadAllText("StorageInfos.json");
            _storageInfos = JsonSerializer.Deserialize<List<StorageInfo>>(json) ?? new List<StorageInfo>();
        }
    }

    /// <summary>
    /// Сохранение всех моделей в файловую систему
    /// </summary>
    public static void SaveAllModels()
    {
    }

    /// <summary>
    /// Добавление модели в общее хранилище объектов конструктора
    /// и сохранение этой модели в файловую систему
    /// </summary>
    /// <param name="model">Сохраняемая модель</param>
    public static void SaveModel(Model model)
    {
        // Добавляем в хранилище конструктора/клиента
        Models.AddOrReplace(model);
        // определяем какого типа эта модель
        var accessType = model.AccessType;
        // находим папку для сохранения
        var storageInfo = StorageInfos.FirstOrDefault(si =>
            si.AccessType == accessType
            && si.ConstructorObjectTypes == ConstructorObjectTypes.Model);
        if (storageInfo != null)
        {
            string pathToSave = storageInfo.Path;
            if (string.IsNullOrEmpty(pathToSave))
            {
                return;
            }

            // Переходим в папку для моделей
            Directory.SetCurrentDirectory(pathToSave);
            // Переходим в папку модели
            string folderName = $"Model_{model.Uuid}";
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }

            Directory.SetCurrentDirectory(folderName);
            string fileName = $"Model_{model.Uuid}.json";

            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            string jsonModel = JsonSerializer.Serialize(model, options);
            File.WriteAllText(fileName, jsonModel);
        }
    }

    /// <summary>
    /// Сохранение всех шаблонов в файловую систему
    /// </summary>
    public static void SaveAllSqlTemplates()
    {
    }

    /// <summary>
    /// Добавление шаблона скрипта в общее хранилище объектов конструктора
    /// и сохранение этого шаблона в файловую систему
    /// </summary>
    /// <param name="sqlTemplate">Сохраняемый шаблон</param>
    public static void SaveSqlTemplate(SqlTemplate sqlTemplate)
    {
        SqlTemplates.AddOrReplace(sqlTemplate);

        // сохраняем тэги шаблона в хранилище объектов конструктора
        var tags = sqlTemplate.Tags;
        foreach (var tag in tags)
        {
            Tags.AddOrReplace(tag);
        }

        var accessType = sqlTemplate.AccessType;

        // находим папку для сохранения
        var storageInfo = StorageInfos.FirstOrDefault(si =>
            si.AccessType == accessType
            && si.ConstructorObjectTypes == ConstructorObjectTypes.SqlTemplate);
        if (storageInfo != null)
        {
            string pathToSave = storageInfo.Path;
            if (string.IsNullOrEmpty(pathToSave))
            {
                return;
            }

            // Переходим в папку для моделей
            Directory.SetCurrentDirectory(pathToSave);
            // Переходим в папку модели
            string folderName = $"SqlTemplate_{sqlTemplate.Uuid}";
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }

            Directory.SetCurrentDirectory(folderName);
            string templateFileName = $"SqlTemplate_{sqlTemplate.Uuid}.json";

            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            string jsonTemplate = JsonSerializer.Serialize(sqlTemplate, options);
            File.WriteAllText(templateFileName, jsonTemplate);

            // Сохраняем приложенные тэги
            string tagsFileName = $"Tags_{sqlTemplate.Uuid}.json";
            string jsonTags = JsonSerializer.Serialize(tags, options);
            File.WriteAllText(tagsFileName, jsonTags);
        }
    }

    /// <summary>
    /// Сохранение всех тегов в файловую систему
    /// </summary>
    public static void SaveAllTags()
    {
    }

    /// <summary>
    /// Добавление тэга в общее хранилище объектов конструктора
    /// и сохранение этого шаблона в файловую систему
    /// </summary>
    /// <param name="tag">Сохраняемый тэг</param>
    public static void SaveTag(Tag tag)
    {
        Tags.AddOrReplace(tag);
    }

    public static void SaveStorage()
    {
        SaveAllModels();
        SaveAllSqlTemplates();
        SaveAllTags();
    }

    public static void SaveStorageInfo()
    {
        JsonSerializerOptions options = new JsonSerializerOptions()
        {
            WriteIndented = true
        };
        string json = JsonSerializer.Serialize(_storageInfos, options);
        if (!string.IsNullOrEmpty(UserDataRootPath))
        {
            Directory.SetCurrentDirectory(UserDataRootPath);
        }

        if (!File.Exists("StorageInfos.json"))
        {
            File.WriteAllText("StorageInfos.json", json);
        }
    }

    public static void InitializeStorage()
    {
        // Коренная папка - UserData
        string userDataFolderName = "UserData";
        if (!Directory.Exists(userDataFolderName))
        {
            Directory.CreateDirectory(userDataFolderName);
        }

        Directory.SetCurrentDirectory(userDataFolderName);
        UserDataRootPath = Directory.GetCurrentDirectory();


        var accessTypes = new[] { AccessType.Public, AccessType.Common, AccessType.Private };
        var constructorTypes = new[] { ConstructorObjectTypes.Model, ConstructorObjectTypes.SqlTemplate };

        foreach (var type in accessTypes)
        {
            string typeFolderName = type.ToString();
            if (!Directory.Exists(typeFolderName))
            {
                Directory.CreateDirectory(typeFolderName);
                Directory.SetCurrentDirectory(typeFolderName);
                foreach (var constructorFolder in constructorTypes)
                {
                    string constructorFolderName = constructorFolder.ToString();
                    if (!Directory.Exists(constructorFolderName))
                    {
                        Directory.CreateDirectory(constructorFolderName);
                        DirectoryInfo directoryInfo = new DirectoryInfo(constructorFolderName);
                        StorageInfo storageInfo = new StorageInfo(type, constructorFolder, directoryInfo.FullName);
                        StorageInfos.Add(storageInfo);
                    }
                }

                // Возвращаемся на уровень выше
                Directory.SetCurrentDirectory(Directory.GetParent(Directory.GetCurrentDirectory()).FullName);
            }
        }

        SaveStorageInfo();
    }

    #endregion
}