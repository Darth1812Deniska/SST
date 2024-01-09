using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.XPath;
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
    private static List<ModelInfo> _modelInfos = new List<ModelInfo>();
    private static List<TemplateInfo> _templateInfos = new List<TemplateInfo>();

    #endregion

    #region properties

    public static ModelsList Models => _models;
    public static SqlTemplatesList SqlTemplates => _sqlTemplates;
    public static TagsList Tags => _tags;
    public static List<StorageInfo> StorageInfos => _storageInfos;
    private static string? UserDataRootPath { get; set; }

    private static List<ModelInfo> ModelInfos => _modelInfos;

    private static List<TemplateInfo> TemplateInfos => _templateInfos;

    #endregion

    #region public methods

    public static void LoadModels()
    {
        _models = new ModelsList();
        foreach (var modelInfo in ModelInfos)
        {
            string modelPath = modelInfo.ModelPath;
            Directory.SetCurrentDirectory(modelPath);
            string modelFileName = modelInfo.ModelFileName;
            string json = File.ReadAllText(modelFileName);
            Model? model = JsonSerializer.Deserialize<Model>(json);
            if (model != null)
            {
                Models.AddOrReplace(model);
            }
        }
    }

    public static void LoadSqlTemplates()
    {
        _sqlTemplates = new SqlTemplatesList();
        foreach (var templateInfo in TemplateInfos)
        {
            string templateFolderPath = templateInfo.TemplateFolderPath;
            Directory.SetCurrentDirectory(templateFolderPath);
            string templateFileName = templateInfo.TemplateFileName;
            //string templateTagsFileName = templateInfo.TemplateTagsFileName;
            string templateJson = File.ReadAllText(templateFileName);
            //string tagsJson = File.ReadAllText(templateTagsFileName);
            SqlTemplate? sqlTemplate = JsonSerializer.Deserialize<SqlTemplate>(templateJson);
            if (sqlTemplate != null)
            {
                SqlTemplates.AddOrReplace(sqlTemplate);
            }
        }
    }

    public static void LoadTags()
    {
        _tags = new TagsList();
    }

    public static void LoadStorage()
    {
        LoadStorageInfos();
        LoadModelInfos();
        LoadTemplateInfos();
        LoadModels();
        LoadSqlTemplates();
        LoadTags();
    }

    private static void LoadStorageInfos()
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

    private static void LoadModelInfos()
    {
        _modelInfos = new List<ModelInfo>();
        var modelsStorages = StorageInfos.Where(si => si.ConstructorObjectTypes == ConstructorObjectTypes.Model).ToList();
        foreach (var storageInfo in modelsStorages)
        {
            string path = storageInfo.Path;
            Directory.SetCurrentDirectory(path);
            if (File.Exists("ModelInfos.json"))
            {
                string json = File.ReadAllText("ModelInfos.json");
                var listToAppend = JsonSerializer.Deserialize<List<ModelInfo>>(json) ?? new List<ModelInfo>();
                _modelInfos.AddRange(listToAppend);
            }
        }
    }

    private static void LoadTemplateInfos()
    {
        _templateInfos = new List<TemplateInfo>();
        var templateStorages = StorageInfos.Where(si => si.ConstructorObjectTypes == ConstructorObjectTypes.SqlTemplate).ToList();
        foreach (var storageInfo in templateStorages)
        {
            string path = storageInfo.Path;
            Directory.SetCurrentDirectory(path);
            if (File.Exists("TemplateInfos.json"))
            {
                string json = File.ReadAllText("TemplateInfos.json");
                var listToAppend = JsonSerializer.Deserialize<List<TemplateInfo>>(json) ?? new List<TemplateInfo>();
                _templateInfos.AddRange(listToAppend);
            }
        }
    }

    /// <summary>
    /// Сохранение всех моделей в файловую систему
    /// </summary>
    public static void SaveAllModels()
    {
        foreach (Model model in Models)
        {
            SaveModelToFileStorage(model);
        }
    }

    private static void SaveModelToFileStorage(Model model)
    {
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
            ModelInfo modelInfo = new ModelInfo(model.Uuid, model.Name, accessType, Directory.GetCurrentDirectory(),
                fileName);
            if (ModelInfos.Exists(x => x.Equals(modelInfo)))
            {
                ModelInfos.Remove(modelInfo);
            }

            ModelInfos.Add(modelInfo);
            SaveModelInfos();
        }
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
        SaveModelToFileStorage(model);
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

            TemplateInfo templateInfo = new TemplateInfo(sqlTemplate.Uuid, sqlTemplate.Name, accessType,
                Directory.GetCurrentDirectory(), templateFileName, tagsFileName);
            if (TemplateInfos.Exists(x => x.Equals(templateInfo)))
            {
                TemplateInfos.Remove(templateInfo);
            }

            TemplateInfos.Add(templateInfo);
            SaveTemplateInfos();
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
        SaveModelInfos();
        SaveTemplateInfos();
        SaveAllModels();
        SaveAllSqlTemplates();
        SaveAllTags();
    }

    private static void SaveModelInfos()
    {
        foreach (var accessType in Enum.GetValues(typeof(AccessType)).Cast<AccessType>())
        {
            // Получить путь для сохранения
            var storageInfo = StorageInfos.FirstOrDefault(si =>
                si.AccessType == accessType && si.ConstructorObjectTypes == ConstructorObjectTypes.Model);
            if (storageInfo == null)
            {
                continue;
            }

            // Слбираем информацию для сохранения
            var infosToSave = ModelInfos.Where(x => x.ModelAccessType == accessType).ToList();

            string pathToSave = storageInfo.Path;
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            string json = JsonSerializer.Serialize(infosToSave, options);
            Directory.SetCurrentDirectory(pathToSave);
            File.WriteAllText("ModelInfos.json", json);
        }
    }

    private static void SaveTemplateInfos()
    {
        foreach (var accessType in Enum.GetValues(typeof(AccessType)).Cast<AccessType>())
        {
            // Получить путь для сохранения
            var storageInfo = StorageInfos.FirstOrDefault(si =>
                si.AccessType == accessType && si.ConstructorObjectTypes == ConstructorObjectTypes.SqlTemplate);
            if (storageInfo == null)
            {
                continue;
            }

            var infosToSave = TemplateInfos.Where(x => x.TemplateAccessType == accessType).ToList();
            string pathToSave = storageInfo.Path;
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            string json = JsonSerializer.Serialize(infosToSave, options);
            Directory.SetCurrentDirectory(pathToSave);
            File.WriteAllText("TemplateInfos.json", json);
        }
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