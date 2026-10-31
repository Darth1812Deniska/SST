using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using SSTCommonClasses.Constants;
using SSTCommonClasses.Constructor.Models;
using SSTCommonClasses.Constructor.Tags;
using SSTCommonClasses.ConstructorStorage.ObjectInfos;
using SSTCommonClasses.ConstructorStorage.ObjectLists;


namespace SSTCommonClasses.ConstructorStorage;

public static class StaticConstructorStorage
{
    #region events

    /// <summary>
    /// Событие возникновения ошибок при загрузке элементов конструктора.
    /// Позволяет не зависеть от UI-библиотек в бизнес-логике: подписчик (UI) сам решает,
    /// как показать пользователю накопленный текст ошибок.
    /// </summary>
    public static event Action<string>? LoadErrorsOccurred;

    private static void ReportLoadErrors(string errorText, string caption)
    {
        if (!string.IsNullOrEmpty(errorText))
        {
            LoadErrorsOccurred?.Invoke(caption + ": " + errorText);
        }
    }

    #endregion

    #region Constants

    /// <summary>
    /// Название коренной папки, в которой хранятся данные об элементах конструктора
    /// </summary>
    private const string UserDataFolderName = "UserData";

    /// <summary>
    /// Название файла содержащего информацию о структуре хранилища конструктора
    /// </summary>
    private const string StorageInfosFileName = "StorageInfos.json";

    /// <summary>
    /// Название файла содержащего информацию о наборе моделей в хранилище
    /// </summary>
    private const string ModelInfosFileName = "ModelInfos.json";

    /// <summary>
    /// Название файла содержащего информацию о наборе sql-шаблонов в хранилище
    /// </summary>
    private const string SqlTemplateInfosFileName = "TemplateInfos.json";

    /// <summary>
    /// Название файла содержащего информацию о наборе шаблонов тэгов в хранилище
    /// </summary>
    private const string TagTemplateInfosFileName = "TagTemplateInfos.json";

    #endregion

    #region fields

    private static ModelsList _models = new();
    private static SqlTemplatesList _sqlTemplates = new();
    private static TagTemplatesList _tagTemplates = new();
    private static List<StorageInfo> _storageInfos = new();
    private static List<ModelInfo> _modelInfos = new();
    private static List<SqlTemplateInfo> _templateInfos = new();
    private static List<TagTemplateInfo> _tagTemplateInfos = new();

    #endregion

    #region public properties

    /// <summary>
    /// Данные о директориях, в которых хранятся элементы конструктора
    /// </summary>
    public static List<StorageInfo> StorageInfos => _storageInfos;

    /// <summary>
    /// Список всех моделей в системе
    /// </summary>
    public static ModelsList Models => _models;

    /// <summary>
    /// Список всех sql-шаблонов в системе
    /// </summary>
    public static SqlTemplatesList SqlTemplates => _sqlTemplates;

    /// <summary>
    /// Список всех шаблонов тэгов в системе
    /// </summary>
    public static TagTemplatesList TagTemplates => _tagTemplates;

    #endregion

    #region private properties

    /// <summary>
    /// Путь к коренной папке с данными об элементах конструктора
    /// </summary>
    private static string? UserDataRootPath { get; set; }

    /// <summary>
    /// Корневая папка данных. По умолчанию находится рядом с исполняемым файлом приложения,
    /// поэтому работа с путями не зависит от текущей директории процесса (Path.Combine вместо Directory.SetCurrentDirectory).
    /// </summary>
    private static string DefaultUserDataRootPath => Path.Combine(AppContext.BaseDirectory, UserDataFolderName);

    /// <summary>
    /// Данные о расположении файлов с информацией о моделях
    /// </summary>
    private static List<ModelInfo> ModelInfos => _modelInfos;

    /// <summary>
    /// Данные о расположении файлов с информацией о sql-шаблонах
    /// </summary>
    private static List<SqlTemplateInfo> SqlTemplateInfos => _templateInfos;

    /// <summary>
    /// Данные о расположении файлов с информацией о шаблонах тэгов
    /// </summary>
    private static List<TagTemplateInfo> TagTemplateInfos => _tagTemplateInfos;

    #endregion

    #region public methods

    /// <summary>
    /// Метод загрузки элементов конструктора из файловой системы
    /// </summary>
    public static void LoadStorage()
    {
        LoadStorageInfos();

        LoadModelInfos();
        LoadSqlTemplateInfos();
        LoadTagTemplateInfos();

        LoadModels();
        LoadSqlTemplates();
        LoadTagsTemplates();
    }

    /// <summary>
    /// Загрузка моделей из файловой системы
    /// </summary>
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
                    string modelFileName = modelInfo.ModelFileName ?? string.Empty;
                    string fullModelFilePath = Path.Combine(modelPath, modelFileName);
                    if (!string.IsNullOrEmpty(modelFileName) && File.Exists(fullModelFilePath))
                    {
                        string json = File.ReadAllText(fullModelFilePath);
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

        ReportLoadErrors(errorText, "Ошибка при загрузке моделей");
    }

    /// <summary>
    /// Загрузка sql-шаблонов из файловой системы
    /// </summary>
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
                    string templateFileName = templateInfo.TemplateInfoFileName ?? string.Empty;
                    string fullTemplateFilePath = Path.Combine(templateFolderPath, templateFileName);
                    if (!string.IsNullOrEmpty(templateFileName) && File.Exists(fullTemplateFilePath))
                    {
                        string templateJson = File.ReadAllText(fullTemplateFilePath);
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

        ReportLoadErrors(errorText, "Ошибка при загрузке шаблонов");
    }

    /// <summary>
    /// Загрузка шаблонов тэгов из файловой системы
    /// </summary>
    public static void LoadTagsTemplates()
    {
        _tagTemplates = new TagTemplatesList();
        string errorText = string.Empty;
        foreach (TagTemplateInfo tagTemplateInfo in TagTemplateInfos)
        {
            try
            {
                string tagTemplatePath = tagTemplateInfo.TagTemplatePath ?? string.Empty;
                if (!string.IsNullOrEmpty(tagTemplatePath) && Directory.Exists(tagTemplatePath))
                {
                    string templateFileName = tagTemplateInfo.TagTemplateFileName ?? string.Empty;
                    string fullTagTemplateFilePath = Path.Combine(tagTemplatePath, templateFileName);
                    if (!string.IsNullOrEmpty(templateFileName) && File.Exists(fullTagTemplateFilePath))
                    {
                        string templateJson = File.ReadAllText(fullTagTemplateFilePath);
                        TagTemplate? tagTemplate = JsonSerializer.Deserialize<TagTemplate>(templateJson);
                        if (tagTemplate != null)
                        {
                            TagTemplates.AddOrReplace(tagTemplate);
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

        ReportLoadErrors(errorText, "Ошибка при загрузке шаблонов тэгов");
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

    /// <summary>
    /// Сохранение всех шаблонов тэгов в файловую систему 
    /// </summary>
    public static void SaveAllTagsTemplates()
    {
        foreach (TagTemplate tagTemplate in TagTemplates)
        {
            SaveTagTemplateToFileStorage(tagTemplate);
        }
    }

    /// <summary>
    /// Добавление шаблона скрипта в общее хранилище объектов конструктора
    /// и сохранение этого шаблона в файловую систему
    /// </summary>
    /// <param name="tagTemplate">Сохраняемый шаблон тэга</param>
    public static void SaveTagTemplate(TagTemplate tagTemplate)
    {
        TagTemplates.AddOrReplace(tagTemplate);
        SaveTagTemplateToFileStorage(tagTemplate);
    }

    /// <summary>
    /// Сохранение данных об элементах консруктрутора в файловую систему
    /// </summary>
    public static void SaveStorage()
    {
        SaveModelInfos();
        SaveTemplateInfos();
        SaveAllModels();
        SaveAllSqlTemplates();
        SaveAllTagsTemplates();
    }

    /// <summary>
    /// Сохранение данных о структуре хранилища клиента
    /// </summary>
    public static void SaveStorageInfo()
    {
        JsonSerializerOptions options = new JsonSerializerOptions();
        string json = JsonSerializer.Serialize(_storageInfos, options);
        string rootPath = UserDataRootPath ?? DefaultUserDataRootPath;
        File.WriteAllText(Path.Combine(rootPath, StorageInfosFileName), json);
    }

    /// <summary>
    /// Инициализация структуры хранилища клиента
    /// </summary>
    public static void InitializeStorage()
    {
        // Коренная папка - UserData (рядом с исполняемым файлом, без изменения текущей директории процесса)
        string userDataRoot = DefaultUserDataRootPath;
        if (!Directory.Exists(userDataRoot))
        {
            Directory.CreateDirectory(userDataRoot);
        }

        UserDataRootPath = userDataRoot;

        AccessType[] accessTypes = new[] { AccessType.Public, AccessType.Common, AccessType.Private };
        ConstructorType[] constructorTypes = new[]
            { ConstructorType.Model, ConstructorType.SqlTemplate, ConstructorType.TagTemplate };

        foreach (AccessType type in accessTypes)
        {
            string typeFolderName = type.ToString();
            string typeFolderPath = Path.Combine(userDataRoot, typeFolderName);
            if (!Directory.Exists(typeFolderPath))
            {
                Directory.CreateDirectory(typeFolderPath);
            }

            foreach (ConstructorType constructorFolder in constructorTypes)
            {
                string constructorFolderName = constructorFolder.ToString();
                string constructorFolderPath = Path.Combine(typeFolderPath, constructorFolderName);
                if (!Directory.Exists(constructorFolderPath))
                {
                    Directory.CreateDirectory(constructorFolderPath);
                }

                StorageInfo storageInfo = new StorageInfo(type, constructorFolder, constructorFolderPath);
                StorageInfos.Add(storageInfo);
            }
        }

        SaveStorageInfo();
    }

    #endregion

    #region private methods

    /// <summary>
    /// Загрузка информации о структуре хранилища 
    /// </summary>
    private static void LoadStorageInfos()
    {
        string rootPath = UserDataRootPath ?? DefaultUserDataRootPath;
        string storageInfosFilePath = Path.Combine(rootPath, StorageInfosFileName);
        if (File.Exists(storageInfosFilePath))
        {
            string json = File.ReadAllText(storageInfosFilePath);
            _storageInfos = JsonSerializer.Deserialize<List<StorageInfo>>(json) ?? new List<StorageInfo>();
        }
    }

    /// <summary>
    /// Загрузка информации о расположении моделей в хранилище
    /// </summary>
    private static void LoadModelInfos()
    {
        _modelInfos = new List<ModelInfo>();
        List<StorageInfo> modelsStorages = StorageInfos.Where(si => si.ConstructorObjectTypes == ConstructorType.Model)
            .ToList();
        foreach (StorageInfo storageInfo in modelsStorages)
        {
            string modelInfosFilePath = Path.Combine(storageInfo.Path, ModelInfosFileName);
            if (File.Exists(modelInfosFilePath))
            {
                string json = File.ReadAllText(modelInfosFilePath);
                List<ModelInfo> listToAppend =
                    JsonSerializer.Deserialize<List<ModelInfo>>(json) ?? new List<ModelInfo>();
                _modelInfos.AddRange(listToAppend);
            }
        }
    }

    /// <summary>
    /// Загрузка информации о расположении sql-шаблонов в хранилище
    /// </summary>
    private static void LoadSqlTemplateInfos()
    {
        _templateInfos = new List<SqlTemplateInfo>();
        List<StorageInfo> templateStorages = StorageInfos
            .Where(si => si.ConstructorObjectTypes == ConstructorType.SqlTemplate)
            .ToList();
        foreach (StorageInfo storageInfo in templateStorages)
        {
            string templateInfosFilePath = Path.Combine(storageInfo.Path, SqlTemplateInfosFileName);
            if (File.Exists(templateInfosFilePath))
            {
                string json = File.ReadAllText(templateInfosFilePath);
                List<SqlTemplateInfo> listToAppend = JsonSerializer.Deserialize<List<SqlTemplateInfo>>(json) ??
                                                     new List<SqlTemplateInfo>();
                _templateInfos.AddRange(listToAppend);
            }
        }
    }

    /// <summary>
    /// Загрузка информации о расположении шаблонов тэгов в хранилище
    /// </summary>
    private static void LoadTagTemplateInfos()
    {
        _tagTemplateInfos = new List<TagTemplateInfo>();
        List<StorageInfo> templateStorages = StorageInfos
            .Where(si => si.ConstructorObjectTypes == ConstructorType.TagTemplate)
            .ToList();
        foreach (StorageInfo storageInfo in templateStorages)
        {
            string tagTemplateInfosFilePath = Path.Combine(storageInfo.Path, TagTemplateInfosFileName);
            if (File.Exists(tagTemplateInfosFilePath))
            {
                string json = File.ReadAllText(tagTemplateInfosFilePath);
                List<TagTemplateInfo> listToAppend = JsonSerializer.Deserialize<List<TagTemplateInfo>>(json) ??
                                                     new List<TagTemplateInfo>();
                _tagTemplateInfos.AddRange(listToAppend);
            }
        }
    }

    /// <summary>
    /// Сохранение модели в хранилище
    /// </summary>
    /// <param name="model">Сохраняемая модель</param>
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

            // Папка для сохранения модели
            string folderName = $"Model_{model.Uuid}";
            string modelFolderPath = Path.Combine(pathToSave, folderName);
            if (!Directory.Exists(modelFolderPath))
            {
                Directory.CreateDirectory(modelFolderPath);
            }

            string fileName = $"{folderName}.json";
            string fullModelFilePath = Path.Combine(modelFolderPath, fileName);

            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };

            string jsonModel = JsonSerializer.Serialize(model, options);

            File.WriteAllText(fullModelFilePath, jsonModel);

            ModelInfo modelInfo = new ModelInfo()
            {
                ModelAccessType = accessType,
                ModelFileName = fileName,
                ModelName = model.Name,
                ModelPath = modelFolderPath,
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

    /// <summary>
    /// Сохранение sql-шаблонов в хранилище
    /// </summary>
    /// <param name="sqlTemplate">Сохраняемый sql-шаблон</param>
    private static void SaveSqlTemplateToFileStorage(SqlTemplate sqlTemplate)
    {
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

            // Находим имя Шаблона без недопустимых симполов
            string sqlTemplateName = RemoveInvalidChars(sqlTemplate.Name);

            // Папка шаблона
            string folderName = sqlTemplateName;
            string templateFolderPath = Path.Combine(pathToSave, folderName);
            if (!Directory.Exists(templateFolderPath))
            {
                Directory.CreateDirectory(templateFolderPath);
            }

            string templateInfoFileName = $"{sqlTemplateName}.json";
            string fullTemplateFilePath = Path.Combine(templateFolderPath, templateInfoFileName);

            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };

            string jsonTemplate = JsonSerializer.Serialize(sqlTemplate, options);
            File.WriteAllText(fullTemplateFilePath, jsonTemplate);

            SqlTemplateInfo templateInfo = new SqlTemplateInfo(sqlTemplate.Uuid, sqlTemplate.Name, accessType,
                templateFolderPath, templateInfoFileName);
            if (SqlTemplateInfos.Exists(x => x.Equals(templateInfo)))
            {
                SqlTemplateInfos.Remove(templateInfo);
            }

            SqlTemplateInfos.Add(templateInfo);
            SaveTemplateInfos();
        }
    }

    /// <summary>
    /// Сохранение шаблона тэга в хранилище
    /// </summary>
    /// <param name="tagTemplate">Сохраняемый шаблон тэга</param>
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

            // Находим имя Шаблона без недопустимых симполов
            string tagTemplateName = RemoveInvalidChars(tagTemplate.Name);
            string templateInfoFileName = $"{tagTemplateName}.json";
            string fullTagTemplateFilePath = Path.Combine(pathToSave, templateInfoFileName);
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            string jsonTemplate = JsonSerializer.Serialize(tagTemplate, options);
            File.WriteAllText(fullTagTemplateFilePath, jsonTemplate);
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

    /// <summary>
    /// Сохранение всей информации о расположениии моделей в хранилище
    /// </summary>
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
            File.WriteAllText(Path.Combine(pathToSave, ModelInfosFileName), json);
        }
    }

    /// <summary>
    /// Сохранение всей информации о расположениии sql-шаблонов в хранилище
    /// </summary>
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
            File.WriteAllText(Path.Combine(pathToSave, SqlTemplateInfosFileName), json);
        }
    }

    /// <summary>
    /// Сохранение всей информации о расположениии шаблонов тэгов в хранилище
    /// </summary>
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
            File.WriteAllText(Path.Combine(pathToSave, TagTemplateInfosFileName), json);
        }
    }

    /// <summary>
    /// Удаление недопустимых знаков из имени файла
    /// </summary>
    /// <param name="filename">Имя файла</param>
    /// <returns>Обработанное имя файла</returns>
    private static string RemoveInvalidChars(string filename)
    {
        return string.Concat(filename.Split(Path.GetInvalidFileNameChars()));
    }

    #endregion
}