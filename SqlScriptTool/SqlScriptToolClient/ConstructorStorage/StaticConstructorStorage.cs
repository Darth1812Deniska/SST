using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SSTCommonClasses.Models;
using SSTCommonClasses.Tags;

namespace SqlScriptToolClient.ConstructorStorage;

public static class StaticConstructorStorage
{
    
    private static List<Model> _models = new List<Model>();
    private static List<SqlTemplate> _sqlTemplates = new List<SqlTemplate>();
    private static List<Tag> _tags = new List<Tag>();

    public static List<Model> Models => _models;

    public static List<SqlTemplate> SqlTemplates => _sqlTemplates;

    public static List<Tag> Tags => _tags;

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
        LoadModels();
        LoadSqlTemplates();
        LoadTags();
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

    public static void InitializeStorage()
    {
        // Коренная папка - UserData
        string userDataFolderName = "UserData";
        if (!Directory.Exists(userDataFolderName))
        {
            Directory.CreateDirectory(userDataFolderName);
        }
        Directory.SetCurrentDirectory(userDataFolderName);

        string[] accessTypeFolders = { "Public", "Common", "Private" };
        string[] constructorObjectFolders = { "Models", "Templates" };
        foreach (string typeFolder in accessTypeFolders)
        {
            if (!Directory.Exists(typeFolder))
            {
                Directory.CreateDirectory(typeFolder);
                Directory.SetCurrentDirectory(typeFolder);
                foreach (var constructorFolder in constructorObjectFolders)
                {
                    if (!Directory.Exists(constructorFolder))
                    {
                        Directory.CreateDirectory(constructorFolder);
                    }
                    
                }
                // Возвращаемся на уровень выше
                Directory.SetCurrentDirectory(Directory.GetParent(Directory.GetCurrentDirectory()).FullName);

            }
        }
    }
}