using System;
using SSTCommonClasses.Constants;

namespace SSTCommonClasses.ConstructorStorage.ObjectInfos;
/// <summary>
/// Класс для описания информации о хранении SQL-шаблона в файловой системе
/// </summary>
public class SqlTemplateInfo
{
    /// <summary>
    /// Уникальный идентификатор SQL-шаблона
    /// </summary>
    public Guid? TemplateUuid { get; }
    /// <summary>
    /// Название сохраненного SQL-шаблона
    /// </summary>
    public string? TemplateName { get; set; }
    /// <summary>
    /// Тип доступа к шаблону
    /// </summary>
    public AccessType? TemplateAccessType { get; set; }
    /// <summary>
    /// Путь сохранения sql-шаблона в файловой системе
    /// </summary>
    public string? TemplateFolderPath { get; set; }
    /// <summary>
    /// Название файла атрибутов sql-шаблона (расширение .json)
    /// </summary>
    public string? TemplateInfoFileName { get; set; }

    /// <summary>
    /// Название файла с текстом sql-шаблона (расширение .sql),
    /// который хранится в той же папке, что и файл атрибутов
    /// </summary>
    public string? TemplateSqlFileName { get; set; }


    /// <summary>
    /// Создание экземпляра с информацией о хранении SQL-шаблона в файловой системе
    /// </summary>
    public SqlTemplateInfo()
    {
    }

    /// <summary>
    /// Создание экземпляра с информацией о хранении SQL-шаблона в файловой системе
    /// </summary>
    /// <param name="templateUuid">Уникальный идентификатор SQL-шаблона</param>
    /// <param name="templateName">Название сохраненного SQL-шаблона</param>
    /// <param name="templateAccessType">Тип доступа к шаблону</param>
    /// <param name="templateFolderPath">Путь сохранения sql-шаблона в файловой системе</param>
    /// <param name="templateFileName">Название файла атрибутов sql-шаблона (.json)</param>
    /// <param name="templateSqlFileName">Название файла с текстом sql-шаблона (.sql)</param>
    public SqlTemplateInfo(Guid templateUuid, string? templateName, AccessType templateAccessType,
        string templateFolderPath, string templateFileName, string templateSqlFileName)
    {
        TemplateUuid = templateUuid;
        TemplateName = templateName;
        TemplateAccessType = templateAccessType;
        TemplateFolderPath = templateFolderPath;
        TemplateInfoFileName = templateFileName;
        TemplateSqlFileName = templateSqlFileName;
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