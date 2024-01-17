using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.XPath;
using SqlScriptToolClient.ConstructorStorage.FileStorageSerializeTemplates;
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
    private static TagTemplatesList _tagTemplates = new TagTemplatesList();
    private static List<StorageInfo> _storageInfos = new List<StorageInfo>();
    private static List<ModelInfo> _modelInfos = new List<ModelInfo>();
    private static List<TemplateInfo> _templateInfos = new List<TemplateInfo>();
    private static List<FileStorageModel> _fileStorageModels;
    private static List<FileStorageModelItem> _fileStorageModelItems;
    private static List<FileStorageSqlTemplate> _fileStorageSqlTemplates;
    private static List<FileStorageTag> _fileStorageTags;

    #endregion

    #region public properties

    public static ModelsList Models => _models;
    public static SqlTemplatesList SqlTemplates => _sqlTemplates;
    public static TagsList Tags => _tags;
    public static TagTemplatesList TagTemplates => _tagTemplates;

    public static List<StorageInfo> StorageInfos => _storageInfos;

    #endregion

    #region private properties

    private static string? UserDataRootPath { get; set; }
    private static List<ModelInfo> ModelInfos => _modelInfos;
    private static List<TemplateInfo> TemplateInfos => _templateInfos;

    //private static List<FileStorageModel> FileStorageModels => _fileStorageModels;
    //private static List<FileStorageModelItem> FileStorageModelItems => _fileStorageModelItems;
    //private static List<FileStorageSqlTemplate> FileStorageSqlTemplates => _fileStorageSqlTemplates;
    //private static List<FileStorageTag> FileStorageTags => _fileStorageTags;

    #endregion

    #region public methods

    public static void LoadModels()
    {
        _models = new ModelsList();
        foreach (var modelInfo in ModelInfos)
        {
            try
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
            catch (Exception e)
            {
                throw e;
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
            string templateFileName = templateInfo.TemplateInfoFileName;
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

    /*public static void LoadTags()
    {
        _tags = new TagsList();
    }*/

    public static void LoadStorage()
    {
        LoadStorageInfos();
        LoadModelInfos();
        LoadTemplateInfos();

        // загрузить шаблоны sql-скриптов и тэгов, которые там используются
        //LoadFileStorageSqlTemplates();
        // Загрузить шаблоны моделей
        //LoadFileStorageModels();

        LoadModels();
        LoadSqlTemplates();
        LoadTagsTemplates();
    }

    public static void LoadTagsTemplates()
    {
        _tagTemplates = new TagTemplatesList();
    }

    /*private static void LoadFileStorageSqlTemplates()
    {
        _fileStorageSqlTemplates = new List<FileStorageSqlTemplate>();
        _fileStorageTags = new List<FileStorageTag>();
        foreach (var templateInfo in TemplateInfos)
        {
            string templateFolderPath = templateInfo.TemplateFolderPath;
            if (Directory.Exists(templateFolderPath))
            {
                Directory.SetCurrentDirectory(templateFolderPath);

                string infoFileName = templateInfo.TemplateInfoFileName;
                //string sqlFileName = templateInfo.TemplateSqlFileName;
                //string tagsFileName = templateInfo.TemplateTagsFileName;

                string infoJson = File.ReadAllText(infoFileName);
                //string sqlText = File.ReadAllText(sqlFileName);
                //string tagsJson = File.ReadAllText(tagsFileName);

                FileStorageSqlTemplate? sqlTemplate = JsonSerializer.Deserialize<FileStorageSqlTemplate>(infoJson);
                if (sqlTemplate != null)
                {
                    FileStorageSqlTemplates.Add(sqlTemplate);
                }

                /*List<FileStorageTag>? tagsList = JsonSerializer.Deserialize<List<FileStorageTag>>(tagsJson);
                if (tagsList != null)
                {
                    FileStorageTags.AddRange(tagsList);
                }#1#
            }
        }
    }*/

    /*private static void LoadFileStorageModels()
    {
        _fileStorageModels = new List<FileStorageModel>();
        _fileStorageModelItems = new List<FileStorageModelItem>();

        foreach (var modelInfo in ModelInfos)
        {
            string modelPath = modelInfo.ModelPath;
            if (Directory.Exists(modelPath))
            {
                Directory.SetCurrentDirectory(modelPath);
                string modelFileName = modelInfo.ModelFileName;
                //string modelItemsFileName = modelInfo.ModelItemsFileName;

                string modelJson = File.ReadAllText(modelFileName);
                //string modelItemsJson = File.ReadAllText(modelItemsFileName);

                FileStorageModel? fileStorageModel = JsonSerializer.Deserialize<FileStorageModel>(modelJson);
                if (fileStorageModel != null)
                {
                    FileStorageModels.Add(fileStorageModel);
                }

                /*List<FileStorageModelItem>? fileStorageModelItems =
                    JsonSerializer.Deserialize<List<FileStorageModelItem>>(modelItemsJson);#1#

                /*if (fileStorageModelItems != null)
                {
                    FileStorageModelItems.AddRange(fileStorageModelItems);
                }#1#
            }
        }
    }*/


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
        foreach (SqlTemplate sqlTemplate in SqlTemplates)
        {
            SaveSqlTemplateToFileStorage(sqlTemplate);
        }
    }

    /// <summary>
    /// Добавление шаблона скрипта в общее хранилище объектов конструктора
    /// и сохранение этого шаблона в файловую систему
    /// </summary>
    /// <param name="sqlTemplate">Сохраняемый шаблон</param>
    public static void SaveSqlTemplate(SqlTemplate sqlTemplate)
    {
        SqlTemplates.AddOrReplace(sqlTemplate);
        SaveSqlTemplateToFileStorage(sqlTemplate);
    }


    /*public static void SaveAllTags()
    {
    }*/


    /*public static void SaveTag(Tag tag)
    {
        Tags.AddOrReplace(tag);
    }*/

    public static void SaveStorage()
    {
        SaveModelInfos();
        SaveTemplateInfos();
        SaveAllModels();
        SaveAllSqlTemplates();
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

    #region private methods

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
        var modelsStorages = StorageInfos.Where(si => si.ConstructorObjectTypes == ConstructorObjectTypes.Model)
            .ToList();
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
        var templateStorages = StorageInfos.Where(si => si.ConstructorObjectTypes == ConstructorObjectTypes.SqlTemplate)
            .ToList();
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
            string modelItemsFileName = $"ModelItems_{model.Uuid}.json";

            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            //FileStorageModel fileStorageModel = FileStorageTemplatesConverter.GenerateFileStorageModel(model);
            string jsonModel = JsonSerializer.Serialize(model, options);
            //List<FileStorageModelItem> fileStorageModelItems =
            //    FileStorageTemplatesConverter.GenerateFileStorageModelItems(model.RootModelItem);
            //string jsonModelItems = JsonSerializer.Serialize(fileStorageModelItems, options);
            File.WriteAllText(fileName, jsonModel);
            //File.WriteAllText(modelItemsFileName, jsonModelItems);
            ModelInfo modelInfo = new ModelInfo(model.Uuid, model.Name, accessType, Directory.GetCurrentDirectory(),
                fileName /*, modelItemsFileName*/);
            if (ModelInfos.Exists(x => x.Equals(modelInfo)))
            {
                ModelInfos.Remove(modelInfo);
            }

            ModelInfos.Add(modelInfo);
            SaveModelInfos();
        }
    }

    private static void SaveSqlTemplateToFileStorage(SqlTemplate sqlTemplate)
    {
        // сохраняем тэги шаблона в хранилище объектов конструктора
        var tags = sqlTemplate.Tags;
        /*foreach (var tag in tags)
        {
            Tags.AddOrReplace(tag);
        }*/

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

            // Переходим в папку для шаблонов
            Directory.SetCurrentDirectory(pathToSave);

            // Находим имя Шаблона без недопустимых симполов
            string sqlTemplateName = RemoveInvalidChars(sqlTemplate.Name);

            // Переходим в папку шаблона
            string folderName = sqlTemplateName;
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }

            Directory.SetCurrentDirectory(folderName);
            //string templateSqlFileName = $"{sqlTemplateName}.sql";
            string templateInfoFileName = $"{sqlTemplateName}.json";

            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };

            /*FileStorageSqlTemplate fileStorageSqlTemplate =
                FileStorageTemplatesConverter.GenerateFileStorageSqlTemplate(sqlTemplate);*/

            string jsonTemplate = JsonSerializer.Serialize(sqlTemplate, options);
            File.WriteAllText(templateInfoFileName, jsonTemplate);

            // Сохраняем шаблон в sql-файл
            //File.WriteAllText(templateSqlFileName, sqlTemplate.SqlText);

            // Сохраняем приложенные тэги
            //string tagsFileName = $"Tags_{sqlTemplate.Uuid}.json";
            //var tagsInfo = tags.Select(FileStorageTemplatesConverter.GenerateFileStorageTag).ToList();
            //string jsonTags = JsonSerializer.Serialize(tagsInfo, options);
            //File.WriteAllText(tagsFileName, jsonTags);

            TemplateInfo templateInfo = new TemplateInfo(sqlTemplate.Uuid, sqlTemplate.Name, accessType,
                Directory.GetCurrentDirectory(), templateInfoFileName /*, templateSqlFileName, tagsFileName*/);
            if (TemplateInfos.Exists(x => x.Equals(templateInfo)))
            {
                TemplateInfos.Remove(templateInfo);
            }

            TemplateInfos.Add(templateInfo);
            SaveTemplateInfos();
        }
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

            // Собираем информацию для сохранения
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

    private static string RemoveInvalidChars(string filename)
    {
        return string.Concat(filename.Split(Path.GetInvalidFileNameChars()));
    }

    #endregion
}