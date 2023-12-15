using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SSTCommonClasses.Constants;
using SSTCommonClasses.Models;
using SSTCommonClasses.Tags;

namespace SqlScriptToolClient.ConstructorStorage;

public static class StaticConstructorStorage
{
    #region fields

    private static List<Model> _models = new List<Model>();
    private static List<SqlTemplate> _sqlTemplates = new List<SqlTemplate>();
    private static List<Tag> _tags = new List<Tag>();
    private static List<StorageInfo> _storageInfos = new List<StorageInfo>();

    #endregion

    #region properties

    public static List<Model> Models => _models;
    public static List<SqlTemplate> SqlTemplates => _sqlTemplates;
    public static List<Tag> Tags => _tags;
    public static List<StorageInfo> StorageInfos => _storageInfos;
    public static string? UserDataRootPath { get; set; }

    #endregion

    #region public methods

    public static void LoadModels()
    {
        _models = new List<Model>();
    }

    public static void LoadSqlTemplates()
    {
        _sqlTemplates = new List<SqlTemplate>();
    }

    public static void LoadTags()
    {
        _tags = new List<Tag>();
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
        string json = File.ReadAllText("StorageInfos.json");
        _storageInfos=JsonSerializer.Deserialize<List<StorageInfo>>(json);
    }

    public static void SaveAllModels()
    {
    }

    public static void SaveAllSqlTemplates()
    {
    }

    public static void SaveAllTags()
    {
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


        var accessTypes = new[] { ModelAccessType.Public, ModelAccessType.Common, ModelAccessType.Private };
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