using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Xml.Linq;
using SqlScriptToolClient.ConstructorStorage.FileStorageSerializeTemplates;
using SqlScriptToolClient.ConstructorStorage.ObjectInfos;
using SqlScriptToolClient.ConstructorStorage.ObjectLists;
using SSTCommonClasses.Constants;
using SSTCommonClasses.Constructor.Models;
using SSTCommonClasses.Constructor.Tags;


namespace SqlScriptToolClient.ConstructorStorage;

public static class StaticConstructorStorage
{
    #region Constants

    private const string StorageInfosFileName = "StorageInfos.json";
    private const string UserDataFolderName = "UserData";
    private const string ModelInfosFileName = "ModelInfos.json";
    private const string SqlTemplateInfosFileName = "TemplateInfos.json";
    private const string TagTemplateInfosFileName = "TagTemplateInfos.json";

    #endregion

    #region fields

    private static ModelsList _models = new();
    private static SqlTemplatesList _sqlTemplates = new();
    private static TagsList _tags = new();
    private static TagTemplatesList _tagTemplates = new();
    private static List<StorageInfo> _storageInfos = new();
    private static List<ModelInfo> _modelInfos = new();
    private static List<SqlTemplateInfo> _templateInfos = new();

    private static List<TagTemplateInfo> _tagTemplateInfos = new();
    //private static List<FileStorageModel> _fileStorageModels;
    //private static List<FileStorageModelItem> _fileStorageModelItems;
    //private static List<FileStorageSqlTemplate> _fileStorageSqlTemplates;
    //private static List<FileStorageTag> _fileStorageTags;

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
    private static List<SqlTemplateInfo> SqlTemplateInfos => _templateInfos;
    private static List<TagTemplateInfo> TagTemplateInfos => _tagTemplateInfos;

    //private static List<FileStorageModel> FileStorageModels => _fileStorageModels;
    //private static List<FileStorageModelItem> FileStorageModelItems => _fileStorageModelItems;
    //private static List<FileStorageSqlTemplate> FileStorageSqlTemplates => _fileStorageSqlTemplates;
    //private static List<FileStorageTag> FileStorageTags => _fileStorageTags;

    #endregion

    #region public methods

    public static void LoadModels()
    {
        _models = new ModelsList();
        string errorText = string.Empty;
        foreach (ModelInfo modelInfo in ModelInfos)
        {
            try
            {
                string modelPath = modelInfo.ModelPath ?? string.Empty;
                if (!string.IsNullOrEmpty(modelPath) && Directory.Exists(modelPath))
                {
                    Directory.SetCurrentDirectory(modelPath);
                    string modelFileName = modelInfo.ModelFileName ?? string.Empty;
                    if (!string.IsNullOrEmpty(modelFileName) && File.Exists(modelFileName))
                    {
                        string json = File.ReadAllText(modelFileName);
                        Model? model = JsonSerializer.Deserialize<Model>(json);
                        if (model != null)
                        {
                            Models.AddOrReplace(model);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                errorText = string.Concat(errorText, "\n", e.Message);
                continue;
            }
        }

        if (!string.IsNullOrEmpty(errorText))
        {
            MessageBox.Show(errorText, "Ошибка при загрузке моделей", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public static void LoadSqlTemplates()
    {
        _sqlTemplates = new SqlTemplatesList();
        string errorText = string.Empty;
        foreach (SqlTemplateInfo templateInfo in SqlTemplateInfos)
        {
            try
            {
                string templateFolderPath = templateInfo.TemplateFolderPath ?? string.Empty;
                if (!string.IsNullOrEmpty(templateFolderPath) && Directory.Exists(templateFolderPath))
                {
                    Directory.SetCurrentDirectory(templateFolderPath);
                    string templateFileName = templateInfo.TemplateInfoFileName ?? string.Empty;
                    if (!string.IsNullOrEmpty(templateFileName) && File.Exists(templateFileName))
                    {
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
            }
            catch (Exception e)
            {
                errorText = string.Concat(errorText, "\n", e.Message);
                continue;
            }
        }
        if (!string.IsNullOrEmpty(errorText))
        {
            MessageBox.Show(errorText, "Ошибка при загрузке шаблонов", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public static void LoadTagsTemplates()
    {
        _tagTemplates = new TagTemplatesList();
        foreach (TagTemplateInfo tagTemplateInfo in TagTemplateInfos)
        {
            string tagTemplatePath = tagTemplateInfo.TagTemplatePath ?? string.Empty;
            if (!string.IsNullOrEmpty(tagTemplatePath) && Directory.Exists(tagTemplatePath))
            {
                Directory.SetCurrentDirectory(tagTemplatePath);
                string templateFileName = tagTemplateInfo.TagTemplateFileName ?? string.Empty;
                if (!string.IsNullOrEmpty(templateFileName) && File.Exists(templateFileName))
                {
                    string templateJson = File.ReadAllText(templateFileName);
                    TagTemplate? tagTemplate = JsonSerializer.Deserialize<TagTemplate>(templateJson);
                    if (tagTemplate != null)
                    {
                        TagTemplates.AddOrReplace(tagTemplate);
                    }
                }
            }
        }
    }

    public static void LoadStorage()
    {
        LoadStorageInfos();
        LoadModelInfos();
        LoadSqlTemplateInfos();
        LoadTagTemplateInfos();

        // загрузить шаблоны sql-скриптов и тэгов, которые там используются
        //LoadFileStorageSqlTemplates();
        // Загрузить шаблоны моделей
        //LoadFileStorageModels();

        LoadModels();
        LoadSqlTemplates();
        LoadTagsTemplates();
    }

    public static void LoadTagTemplateInfos()
    {
        _tagTemplateInfos = new List<TagTemplateInfo>();
        List<StorageInfo> templateStorages = StorageInfos
            .Where(si => si.ConstructorObjectTypes == ConstructorType.TagTemplate)
            .ToList();
        foreach (StorageInfo storageInfo in templateStorages)
        {
            string path = storageInfo.Path;
            Directory.SetCurrentDirectory(path);
            if (File.Exists(TagTemplateInfosFileName))
            {
                string json = File.ReadAllText(TagTemplateInfosFileName);
                List<TagTemplateInfo> listToAppend = JsonSerializer.Deserialize<List<TagTemplateInfo>>(json) ??
                                                     new List<TagTemplateInfo>();
                _tagTemplateInfos.AddRange(listToAppend);
            }
        }
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

    public static void SaveAllTagsTemplates()
    {
        foreach (TagTemplate tagTemplate in TagTemplates)
        {
            SaveTagTemplateToFileStorage(tagTemplate);
        }
    }

    public static void SaveTagTemplate(TagTemplate tagTemplate)
    {
        TagTemplates.AddOrReplace(tagTemplate);
        SaveTagTemplateToFileStorage(tagTemplate);
    }

    public static void SaveStorage()
    {
        SaveModelInfos();
        SaveTemplateInfos();
        SaveAllModels();
        SaveAllSqlTemplates();
        SaveAllTagsTemplates();
    }

    public static void SaveStorageInfo()
    {
        JsonSerializerOptions options = new JsonSerializerOptions();
        string json = JsonSerializer.Serialize(_storageInfos, options);
        if (!string.IsNullOrEmpty(UserDataRootPath))
        {
            Directory.SetCurrentDirectory(UserDataRootPath);
        }

        File.WriteAllText(StorageInfosFileName, json);
    }

    public static void InitializeStorage()
    {
        // Коренная папка - UserData
        string userDataFolderName = UserDataFolderName;
        if (!Directory.Exists(userDataFolderName))
        {
            Directory.CreateDirectory(userDataFolderName);
        }

        Directory.SetCurrentDirectory(userDataFolderName);
        UserDataRootPath = Directory.GetCurrentDirectory();


        AccessType[] accessTypes = new[] { AccessType.Public, AccessType.Common, AccessType.Private };
        ConstructorType[] constructorTypes = new[]
            { ConstructorType.Model, ConstructorType.SqlTemplate, ConstructorType.TagTemplate };

        foreach (AccessType type in accessTypes)
        {
            string typeFolderName = type.ToString();
            if (!Directory.Exists(typeFolderName))
            {
                Directory.CreateDirectory(typeFolderName);
            }

            Directory.SetCurrentDirectory(typeFolderName);
            foreach (ConstructorType constructorFolder in constructorTypes)
            {
                string constructorFolderName = constructorFolder.ToString();
                if (!Directory.Exists(constructorFolderName))
                {
                    Directory.CreateDirectory(constructorFolderName);
                }

                DirectoryInfo directoryInfo = new DirectoryInfo(constructorFolderName);
                StorageInfo storageInfo = new StorageInfo(type, constructorFolder, directoryInfo.FullName);
                StorageInfos.Add(storageInfo);
            }

            // Возвращаемся на уровень выше
            var parentDirectory = Directory.GetParent(Directory.GetCurrentDirectory());
            if (parentDirectory != null)
            {
                Directory.SetCurrentDirectory(parentDirectory.FullName);
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
        if (File.Exists(StorageInfosFileName))
        {
            string json = File.ReadAllText(StorageInfosFileName);
            _storageInfos = JsonSerializer.Deserialize<List<StorageInfo>>(json) ?? new List<StorageInfo>();
        }
    }

    private static void LoadModelInfos()
    {
        _modelInfos = new List<ModelInfo>();
        List<StorageInfo> modelsStorages = StorageInfos.Where(si => si.ConstructorObjectTypes == ConstructorType.Model)
            .ToList();
        foreach (StorageInfo storageInfo in modelsStorages)
        {
            string path = storageInfo.Path;
            Directory.SetCurrentDirectory(path);
            if (File.Exists(ModelInfosFileName))
            {
                string json = File.ReadAllText(ModelInfosFileName);
                List<ModelInfo> listToAppend =
                    JsonSerializer.Deserialize<List<ModelInfo>>(json) ?? new List<ModelInfo>();
                _modelInfos.AddRange(listToAppend);
            }
        }
    }

    private static void LoadSqlTemplateInfos()
    {
        _templateInfos = new List<SqlTemplateInfo>();
        List<StorageInfo> templateStorages = StorageInfos
            .Where(si => si.ConstructorObjectTypes == ConstructorType.SqlTemplate)
            .ToList();
        foreach (StorageInfo storageInfo in templateStorages)
        {
            string path = storageInfo.Path;
            Directory.SetCurrentDirectory(path);
            if (File.Exists(SqlTemplateInfosFileName))
            {
                string json = File.ReadAllText(SqlTemplateInfosFileName);
                List<SqlTemplateInfo> listToAppend = JsonSerializer.Deserialize<List<SqlTemplateInfo>>(json) ??
                                                     new List<SqlTemplateInfo>();
                _templateInfos.AddRange(listToAppend);
            }
        }
    }

    private static void SaveModelToFileStorage(Model model)
    {
        // определяем какого типа эта модель
        AccessType accessType = model.AccessType;
        // находим папку для сохранения
        StorageInfo? storageInfo = StorageInfos.FirstOrDefault(si =>
            si.AccessType == accessType
            && si.ConstructorObjectTypes == ConstructorType.Model);
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
            ModelInfo modelInfo = new ModelInfo()
            {
                ModelAccessType = accessType,
                ModelFileName = fileName,
                ModelName = model.Name,
                ModelPath = Directory.GetCurrentDirectory(),
                ModelUuid = model.Uuid
            };

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
        List<Tag> tags = sqlTemplate.Tags;
        /*foreach (var tag in tags)
        {
            Tags.AddOrReplace(tag);
        }*/

        AccessType accessType = sqlTemplate.AccessType;

        // находим папку для сохранения
        StorageInfo? storageInfo = StorageInfos.FirstOrDefault(si =>
            si.AccessType == accessType
            && si.ConstructorObjectTypes == ConstructorType.SqlTemplate);
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

            SqlTemplateInfo templateInfo = new SqlTemplateInfo(sqlTemplate.Uuid, sqlTemplate.Name, accessType,
                Directory.GetCurrentDirectory(), templateInfoFileName /*, templateSqlFileName, tagsFileName*/);
            if (SqlTemplateInfos.Exists(x => x.Equals(templateInfo)))
            {
                SqlTemplateInfos.Remove(templateInfo);
            }

            SqlTemplateInfos.Add(templateInfo);
            SaveTemplateInfos();
        }
    }

    private static void SaveTagTemplateToFileStorage(TagTemplate tagTemplate)
    {
        AccessType accessType = tagTemplate.AccessType;

        // находим папку для сохранения
        StorageInfo? storageInfo = StorageInfos.FirstOrDefault(si =>
            si.AccessType == accessType
            && si.ConstructorObjectTypes == ConstructorType.TagTemplate);
        if (storageInfo != null)
        {
            string pathToSave = storageInfo.Path;
            if (string.IsNullOrEmpty(pathToSave))
            {
                return;
            }

            // Переходим в папку для шаблонов тегов
            Directory.SetCurrentDirectory(pathToSave);

            // Находим имя Шаблона без недопустимых симполов
            string tagTemplateName = RemoveInvalidChars(tagTemplate.Name);
            string templateInfoFileName = $"{tagTemplateName}.json";
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            string jsonTemplate = JsonSerializer.Serialize(tagTemplate, options);
            File.WriteAllText(templateInfoFileName, jsonTemplate);
            TagTemplateInfo tagTemplateInfo = new TagTemplateInfo(
                tagTemplate.Uuid, tagTemplate.Name, accessType, pathToSave, templateInfoFileName);

            if (TagTemplateInfos.Exists(x => x.Equals(tagTemplateInfo)))
            {
                TagTemplateInfos.Remove(tagTemplateInfo);
            }

            TagTemplateInfos.Add(tagTemplateInfo);
            SaveTagTemplateInfos();
        }
    }

    private static void SaveModelInfos()
    {
        foreach (AccessType accessType in Enum.GetValues(typeof(AccessType)).Cast<AccessType>())
        {
            // Получить путь для сохранения
            StorageInfo? storageInfo = StorageInfos.FirstOrDefault(si =>
                si.AccessType == accessType && si.ConstructorObjectTypes == ConstructorType.Model);
            if (storageInfo == null)
            {
                continue;
            }

            // Собираем информацию для сохранения
            List<ModelInfo> infosToSave = ModelInfos.Where(x => x.ModelAccessType == accessType).ToList();

            string pathToSave = storageInfo.Path;
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            string json = JsonSerializer.Serialize(infosToSave, options);
            Directory.SetCurrentDirectory(pathToSave);
            File.WriteAllText(ModelInfosFileName, json);
        }
    }

    private static void SaveTemplateInfos()
    {
        foreach (AccessType accessType in Enum.GetValues(typeof(AccessType)).Cast<AccessType>())
        {
            // Получить путь для сохранения
            StorageInfo? storageInfo = StorageInfos.FirstOrDefault(si =>
                si.AccessType == accessType && si.ConstructorObjectTypes == ConstructorType.SqlTemplate);
            if (storageInfo == null)
            {
                continue;
            }

            List<SqlTemplateInfo> infosToSave =
                SqlTemplateInfos.Where(x => x.TemplateAccessType == accessType).ToList();
            string pathToSave = storageInfo.Path;
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            string json = JsonSerializer.Serialize(infosToSave, options);
            Directory.SetCurrentDirectory(pathToSave);
            File.WriteAllText(SqlTemplateInfosFileName, json);
        }
    }

    private static void SaveTagTemplateInfos()
    {
        foreach (AccessType accessType in Enum.GetValues(typeof(AccessType)).Cast<AccessType>())
        {
            // Получить путь для сохранения
            StorageInfo? storageInfo = StorageInfos.FirstOrDefault(si =>
                si.AccessType == accessType
                && si.ConstructorObjectTypes == ConstructorType.TagTemplate);
            if (storageInfo == null)
            {
                continue;
            }

            List<TagTemplateInfo> infosToSave = TagTemplateInfos
                .Where(x => x.TagTemplateAccessType == accessType).ToList();
            string pathToSave = storageInfo.Path;
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            string json = JsonSerializer.Serialize(infosToSave, options);
            Directory.SetCurrentDirectory(pathToSave);
            File.WriteAllText(TagTemplateInfosFileName, json);
        }
    }

    private static string RemoveInvalidChars(string filename)
    {
        return string.Concat(filename.Split(Path.GetInvalidFileNameChars()));
    }

    #endregion
}