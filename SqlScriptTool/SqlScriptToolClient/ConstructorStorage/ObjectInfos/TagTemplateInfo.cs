using SSTCommonClasses.Constants;
using System;

namespace SqlScriptToolClient.ConstructorStorage.ObjectInfos;

/// <summary>
/// Класс для описания информации о хранении шаблонов тэгов в файловой системе
/// </summary>
public class TagTemplateInfo
{
    /// <summary>
    /// Уникальный идентификатор шаблона тэга
    /// </summary>
    public Guid? TagTemplateUuid { get; }

    /// <summary>
    /// Наименование шаблона тэга
    /// </summary>
    public string? TagTemplateName { get; set; }

    /// <summary>
    /// Тип доступа к шаблону тэга
    /// </summary>
    public AccessType? TagTemplateAccessType { get; set; }

    /// <summary>
    /// Путь к папке с шаблоном тэга
    /// </summary>
    public string? TagTemplatePath { get; set; }

    /// <summary>
    /// Название файла содержащего шаблон тэга
    /// </summary>
    public string? TagTemplateFileName { get; set; }

    /// <summary>
    /// Создание объекта с инфрамацией о хранении шаблонов тэгов в файловой системе
    /// </summary>
    public TagTemplateInfo()
    {
    }

    /// <summary>
    /// Создание объекта с инфрамацией о хранении шаблонов тэгов в файловой системе
    /// </summary>
    /// <param name="tagTemplateUuid">Уникальный идентификатор шаблона тэга</param>
    /// <param name="tagTemplateName">Наименование шаблона тэга</param>
    /// <param name="tagTemplateAccessType">Тип доступа к шаблону тэга</param>
    /// <param name="tagTemplatePath">Путь к папке с шаблоном тэга</param>
    /// <param name="tagTemplateFileName">Название файла содержащего шаблон тэга</param>
    public TagTemplateInfo(Guid tagTemplateUuid, string? tagTemplateName, AccessType tagTemplateAccessType,
        string tagTemplatePath, string tagTemplateFileName)
    {
        TagTemplateUuid = tagTemplateUuid;
        TagTemplateName = tagTemplateName;
        TagTemplateAccessType = tagTemplateAccessType;
        TagTemplatePath = tagTemplatePath;
        TagTemplateFileName = tagTemplateFileName;
    }

    public bool Equals(TagTemplateInfo? other)
    {
        if (other == null)
            return false;
        return TagTemplateUuid == other.TagTemplateUuid;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }

        return obj is TagTemplateInfo model && Equals(model);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TagTemplateUuid);
    }
}