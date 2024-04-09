using System;
using SSTCommonClasses.Constants;

namespace SqlScriptToolClient.ConstructorStorage.ObjectInfos;
/// <summary>
/// Класс для описания информации о хранении моделей в файловой системе
/// </summary>
public class SqlTemplateInfo
{
    /// <summary>
    /// УНикальный идентификатор SQL-шаблона
    /// </summary>
    public Guid? TemplateUuid { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? TemplateName { get; set; }
    public AccessType? TemplateAccessType { get; set; }
    public string? TemplateFolderPath { get; set; }
    public string? TemplateInfoFileName { get; set; }
    //public string TemplateSqlFileName { get; set; }
    //public string TemplateTagsFileName { get; set; }

    public SqlTemplateInfo()
    {
    }

    public SqlTemplateInfo(Guid templateUuid, string? templateName, AccessType templateAccessType,
        string templateFolderPath, string templateFileName/*, string templateSqlFileName, string templateTagsFileName*/)
    {
        TemplateUuid = templateUuid;
        TemplateName = templateName;
        TemplateAccessType = templateAccessType;
        TemplateFolderPath = templateFolderPath;
        TemplateInfoFileName = templateFileName;
        //TemplateSqlFileName = templateSqlFileName;
        //TemplateTagsFileName = templateTagsFileName;
        
    }

    public bool Equals(SqlTemplateInfo? other)
    {
        if (other == null)
            return false;
        return TemplateUuid == other.TemplateUuid;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }

        return obj is SqlTemplateInfo model && Equals(model);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TemplateUuid);
    }
}