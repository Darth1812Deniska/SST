using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using SSTCommonClasses.Constants;
using SSTCommonClasses.Constructor.Models;
using SSTCommonClasses.Constructor.Tags;
using SSTCommonClasses.ConstructorStorage.FileStorageDto;
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
    /// Метод загрузки элементов конструктора из файловой системы.
    /// Выгрузка выполняется в два этапа:
    /// 1. Чтение всех DTO из файловой системы и восстановление объектов с неразрешенными ссылками;
    /// 2. Проход по всем объектам и разрешение ссылок (Uuid) уже по загруженным в память DTO.
    /// Двухэтапная загрузка нужна потому, что ссылки между объектами могут быть циклическими
    /// (sql-шаблон -> тэг -> дочерний sql-шаблон -> тэг ...), поэтому на этапе чтения файла
    /// разрешить ссылку по Uuid корректным объектом невозможно.
    /// </summary>
    public static void LoadStorage()
    {
        LoadStorageInfos();

        LoadModelInfos();
        LoadSqlTemplateInfos();
        LoadTagTemplateInfos();

        // Этап 1: читаем все файлы и восстанавливаем объекты с неразрешенными ссылками
        List<Exception>? loadExceptions = null;
        LoadSqlTemplates(out loadExceptions);
        LoadTagsTemplates(out loadExceptions);
        LoadModels(out loadExceptions);

        // Этап 2: проходим по всем загруженным в память DTO и разрешаем ссылки по уникальным идентификаторам
        ResolveLoadedReferences(out loadExceptions);

        ReportLoadErrors(BuildErrorText(loadExceptions), "Ошибка при загрузке элементов конструктора");
    }

    /// <summary>
    /// Загрузка моделей из файловой системы (первый этап выгрузки: чтение файлов без разрешения ссылок).
    /// Ссылки на sql-шаблоны разрешаются на втором этапе — <see cref="ResolveLoadedReferences"/>.
    /// </summary>
    public static void LoadModels()
    {
        LoadModels(out _);
    }

    private static void LoadModels(out List<Exception>? exceptions)
    {
        List<Exception> caught = new();
        exceptions = caught;
        _models = new ModelsList();
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
                        ModelFileDto? modelDto = JsonSerializer.Deserialize<ModelFileDto>(json);
                        if (modelDto != null)
                        {
                            // Ссылки пока не разрешаем — связанные объекты могут еще не быть загружены
                            Model model = modelDto.FromDto(ResolveNone, null);
                            Models.AddOrReplace(model);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                caught.Add(e);
            }
        }
    }

    /// <summary>
    /// Загрузка sql-шаблонов из файловой системы (первый этап выгрузки).
    /// Атрибуты шаблона читаются из файла .json, текст скрипта — из файла .sql,
    /// расположенного в той же папке, что и файл атрибутов.
    /// Ссылки на связанные объекты разрешаются на втором этапе — <see cref="ResolveLoadedReferences"/>.
    /// </summary>
    public static void LoadSqlTemplates()
    {
        LoadSqlTemplates(out _);
    }

    private static void LoadSqlTemplates(out List<Exception>? exceptions)
    {
        List<Exception> caught = new();
        exceptions = caught;
        _sqlTemplates = new SqlTemplatesList();
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
                        SqlTemplateFileDto? templateDto = JsonSerializer.Deserialize<SqlTemplateFileDto>(templateJson);
                        if (templateDto != null)
                        {
                            // Читаем текст sql-шаблона из файла .sql в той же папке, что и файл атрибутов
                            string sqlText = ReadSqlTemplateText(templateFolderPath,
                                templateInfo.TemplateSqlFileName, templateDto.Name);

                            // Ссылки пока не разрешаем — связанные объекты могут еще не быть загружены
                            SqlTemplate sqlTemplate = templateDto.FromDto(ResolveNone, sqlText);
                            SqlTemplates.AddOrReplace(sqlTemplate);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                caught.Add(e);
            }
        }
    }

    /// <summary>
    /// Загрузка шаблонов тэгов из файловой системы (первый этап выгрузки).
    /// Ссылки на связанные объекты разрешаются на втором этапе — <see cref="ResolveLoadedReferences"/>.
    /// </summary>
    public static void LoadTagsTemplates()
    {
        LoadTagsTemplates(out _);
    }

    private static void LoadTagsTemplates(out List<Exception>? exceptions)
    {
        List<Exception> caught = new();
        exceptions = caught;
        _tagTemplates = new TagTemplatesList();
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
                        TagTemplateFileDto? tagTemplateDto = JsonSerializer.Deserialize<TagTemplateFileDto>(templateJson);
                        if (tagTemplateDto != null)
                        {
                            // Ссылки пока не разрешаем — связанные объекты могут еще не быть загружены
                            TagTemplate tagTemplate = tagTemplateDto.FromDto(ResolveNone);
                            TagTemplates.AddOrReplace(tagTemplate);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                caught.Add(e);
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
    /// Сохранение модели в хранилище.
    /// В файл .json сериализуются только атрибуты модели (DTO), при этом узлы дерева
    /// содержат ссылки на sql-шаблоны по уникальному идентификатору, а не объекты целиком.
    /// Каждый sql-шаблон, на который ссылается модель, дополнительно сохраняется в файловую систему.
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

            // Сначала сохраняем все шаблоны, на которые ссылается модель (sql-шаблоны узлов,
            // дочерние sql-шаблоны тэгов и шаблоны тэгов),
            // чтобы в файле модели остались только ссылки по уникальным идентификаторам
            foreach (SqlTemplate referencedTemplate in EnumerateModelSqlTemplates(model))
            {
                SaveSqlTemplateToFileStorage(referencedTemplate);
            }

            foreach (TagTemplate referencedTagTemplate in EnumerateModelTagTemplates(model))
            {
                SaveTagTemplateToFileStorage(referencedTagTemplate);
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

            ModelFileDto modelDto = model.ToDto();
            string jsonModel = JsonSerializer.Serialize(modelDto, options);

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
    /// Сохранение sql-шаблона в хранилище.
    /// Атрибуты шаблона (включая тэги как ссылки по уникальным идентификаторам) сохраняются
    /// в файл с расширением .json, а текст самого sql-шаблона — в файл с расширением .sql,
    /// размещенный в той же папке, что и файл атрибутов.
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

            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };

            // Файл атрибутов шаблона (.json) — без текста скрипта и без вложенных объектов целиком
            string templateInfoFileName = $"{sqlTemplateName}.json";
            string fullTemplateFilePath = Path.Combine(templateFolderPath, templateInfoFileName);
            SqlTemplateFileDto templateDto = sqlTemplate.ToDto();
            string jsonTemplate = JsonSerializer.Serialize(templateDto, options);
            File.WriteAllText(fullTemplateFilePath, jsonTemplate);

            // Файл с текстом sql-шаблона (.sql) — в той же папке, что и файл атрибутов
            string templateSqlFileName = $"{sqlTemplateName}.sql";
            string fullTemplateSqlFilePath = Path.Combine(templateFolderPath, templateSqlFileName);
            File.WriteAllText(fullTemplateSqlFilePath, sqlTemplate.SqlText ?? string.Empty);

            SqlTemplateInfo templateInfo = new SqlTemplateInfo(sqlTemplate.Uuid, sqlTemplate.Name, accessType,
                templateFolderPath, templateInfoFileName, templateSqlFileName);
            if (SqlTemplateInfos.Exists(x => x.Equals(templateInfo)))
            {
                SqlTemplateInfos.Remove(templateInfo);
            }

            SqlTemplateInfos.Add(templateInfo);
            SaveTemplateInfos();
        }
    }

    /// <summary>
    /// Сохранение шаблона тэга в хранилище.
    /// В файл .json сериализуются только атрибуты шаблона тэга, привязанный sql-шаблон
    /// сохраняется отдельным объектом, а в файле хранится ссылка на него по уникальному идентификатору.
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

            // Сначала сохраняем привязанный sql-шаблон, чтобы в файле осталась только ссылка по Uuid
            if (tagTemplate.SqlTemplate != null)
            {
                SaveSqlTemplateToFileStorage(tagTemplate.SqlTemplate);
            }

            // Находим имя Шаблона без недопустимых симполов
            string tagTemplateName = RemoveInvalidChars(tagTemplate.Name);
            string templateInfoFileName = $"{tagTemplateName}.json";
            string fullTagTemplateFilePath = Path.Combine(pathToSave, templateInfoFileName);
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            TagTemplateFileDto tagTemplateDto = tagTemplate.ToDto();
            string jsonTemplate = JsonSerializer.Serialize(tagTemplateDto, options);
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

    /// <summary>
    /// Чтение текста sql-шаблона из файла .sql, размещенного в той же папке, что и файл атрибутов.
    /// Если имя файла текста не задано (например, для данных, сохраненных в старом формате),
    /// выполняется поиск одноименного файла с расширением .sql.
    /// </summary>
    /// <param name="templateFolderPath">Папка, содержащая файл атрибутов шаблона</param>
    /// <param name="templateSqlFileName">Название файла с текстом шаблона из информации о хранилище</param>
    /// <param name="templateName">Название шаблона (резервный вариант имени файла)</param>
    /// <returns>Текст sql-шаблона либо пустая строка, если файл текста отсутствует</returns>
    private static string ReadSqlTemplateText(string templateFolderPath, string? templateSqlFileName, string? templateName)
    {
        if (!string.IsNullOrEmpty(templateSqlFileName))
        {
            string fullSqlFilePath = Path.Combine(templateFolderPath, templateSqlFileName);
            if (File.Exists(fullSqlFilePath))
            {
                return File.ReadAllText(fullSqlFilePath);
            }
        }

        // Резервный поиск: файл .sql с тем же именем, что и файл атрибутов, в той же папке
        if (!string.IsNullOrEmpty(templateName))
        {
            string fallbackSqlFileName = RemoveInvalidChars(templateName) + ".sql";
            string fallbackSqlFilePath = Path.Combine(templateFolderPath, fallbackSqlFileName);
            if (File.Exists(fallbackSqlFilePath))
            {
                return File.ReadAllText(fallbackSqlFilePath);
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// Разрешение ссылки на объект конструктора по его уникальному идентификатору.
    /// Используется при загрузке DTO из файловой системы: вместо сериализованных объектов
    /// целиком в файлы записываются Uuid, которые при загрузке сопоставляются с объектами хранилища.
    /// </summary>
    /// <param name="uuid">Уникальный идентификатор объекта</param>
    /// <returns>Найденный объект (SqlTemplate или TagTemplate) либо null, если объект отсутствует</returns>
    private static object? ResolveConstructorObject(Guid uuid)
    {
        SqlTemplate? sqlTemplate = SqlTemplates.GetById(uuid);
        if (sqlTemplate != null)
        {
            return sqlTemplate;
        }

        TagTemplate? tagTemplate = TagTemplates.GetById(uuid);
        return tagTemplate;
    }

    /// <summary>
    /// Пустой resolver для первого этапа выгрузки: ссылки не разрешаются,
    /// так как связанные объекты могут еще не быть загружены из файловой системы.
    /// </summary>
    private static object? ResolveNone(Guid uuid)
    {
        return null;
    }

    /// <summary>
    /// Второй этап выгрузки: проход по всем объектам, загруженным в память на первом этапе,
    /// и разрешение ссылок (Uuid) по уже загруженным DTO. Выполняется после загрузки всех
    /// типов объектов, поэтому корректно обрабатываются в том числе циклические ссылки
    /// (sql-шаблон -> тэг -> дочерний sql-шаблон и т.д.).
    /// </summary>
    private static void ResolveLoadedReferences(out List<Exception>? exceptions)
    {
        List<Exception> caught = new();
        exceptions = caught;

        foreach (SqlTemplate sqlTemplate in SqlTemplates)
        {
            try
            {
                foreach (Tag tag in sqlTemplate.Tags)
                {
                    if (tag.TagTemplate == null && tag.TagTemplateUuid.HasValue)
                    {
                        tag.TagTemplate = TagTemplates.GetById(tag.TagTemplateUuid.Value);
                    }

                    if (tag.SqlTemplate == null && tag.SqlTemplateUuid.HasValue)
                    {
                        tag.SqlTemplate = SqlTemplates.GetById(tag.SqlTemplateUuid.Value);
                    }
                }
            }
            catch (Exception e)
            {
                caught.Add(e);
            }
        }

        foreach (TagTemplate tagTemplate in TagTemplates)
        {
            try
            {
                if (tagTemplate.SqlTemplate == null && tagTemplate.SqlTemplateUuid.HasValue)
                {
                    tagTemplate.SqlTemplate = SqlTemplates.GetById(tagTemplate.SqlTemplateUuid.Value);
                }
            }
            catch (Exception e)
            {
                caught.Add(e);
            }
        }

        foreach (Model model in Models)
        {
            try
            {
                ResolveModelItemReferences(model.RootModelItem);
            }
            catch (Exception e)
            {
                caught.Add(e);
            }
        }
    }

    /// <summary>
    /// Рекурсивное разрешение ссылок узлов дерева модели на sql-шаблоны по уникальным идентификаторам
    /// </summary>
    private static void ResolveModelItemReferences(ModelItem modelItem)
    {
        // Ленивый getter SqlTemplate создавал новый пустой шаблон при обращении к узлу,
        // который ссылается на сохраненный шаблон по Uuid. Теперь неразрешенная ссылка
        // отличается от Guid.Empty, а отсутствующий шаблон — null.
        if (modelItem.HasSavedSqlTemplateReference && modelItem.SqlTemplate == null)
        {
            modelItem.SqlTemplate = SqlTemplates.GetById(modelItem.SavedSqlTemplateUuid);
        }

        foreach (ModelItem childModelItem in modelItem.ChildModelItems.Values)
        {
            ResolveModelItemReferences(childModelItem);
        }
    }

    /// <summary>
    /// Формирование накопленного текста ошибок загрузки из списка исключений
    /// </summary>
    private static string BuildErrorText(List<Exception>? exceptions)
    {
        if (exceptions == null || exceptions.Count == 0)
        {
            return string.Empty;
        }

        return string.Concat(exceptions.Select(e => "\n" + e.Message));
    }

    /// <summary>
    /// Обход всех sql-шаблонов, на которые ссылается модель (корневые шаблоны узлов дерева
    /// и дочерние шаблоны тэгов), без зацикливания на повторяющихся ссылках
    /// </summary>
    /// <param name="model">Модель, чьи связанные шаблоны нужно обойти</param>
    /// <returns>Неповторяющаяся последовательность sql-шаблонов модели</returns>
    private static IEnumerable<SqlTemplate> EnumerateModelSqlTemplates(Model model)
    {
        HashSet<Guid> visited = new();
        return EnumerateModelSqlTemplatesIterator(model.RootModelItem, visited);
    }

    private static IEnumerable<SqlTemplate> EnumerateModelSqlTemplatesIterator(ModelItem modelItem, HashSet<Guid> visited)
    {
        if (modelItem.SqlTemplate != null && visited.Add(modelItem.SqlTemplate.Uuid))
        {
            yield return modelItem.SqlTemplate;

            foreach (Tag tag in modelItem.SqlTemplate.Tags)
            {
                if (tag.SqlTemplate != null && visited.Add(tag.SqlTemplate.Uuid))
                {
                    yield return tag.SqlTemplate;
                }
            }
        }

        foreach (ModelItem childModelItem in modelItem.ChildModelItems.Values)
        {
            foreach (SqlTemplate template in EnumerateModelSqlTemplatesIterator(childModelItem, visited))
            {
                yield return template;
            }
        }
    }

    /// <summary>
    /// Обход всех шаблонов тэгов, на которые ссылаются тэги sql-шаблонов модели,
    /// без зацикливания на повторяющихся ссылках
    /// </summary>
    /// <param name="model">Модель, чьи связанные шаблоны тэгов нужно обойти</param>
    /// <returns>Неповторяющаяся последовательность шаблонов тэгов модели</returns>
    private static IEnumerable<TagTemplate> EnumerateModelTagTemplates(Model model)
    {
        HashSet<Guid> visited = new();
        foreach (SqlTemplate sqlTemplate in EnumerateModelSqlTemplatesIterator(model.RootModelItem, new HashSet<Guid>()))
        {
            foreach (Tag tag in sqlTemplate.Tags)
            {
                if (tag.TagTemplate != null && visited.Add(tag.TagTemplate.Uuid))
                {
                    yield return tag.TagTemplate;
                }
            }
        }
    }

    #endregion
}