using System;
using SSTCommonClasses.Constants;

namespace SqlScriptToolClient.ConstructorStorage.ObjectInfos;

public class TemplateInfo
{
    public Guid TemplateUuid { get; set; }
    public string TemplateName { get; set; }
    public AccessType TemplateAccessType { get; set; }
    public string TemplateFolderPath { get; set; }
    public string TemplateFileName { get; set; }
    public string TemplateTagsFileName { get; set; }

    public TemplateInfo()
    {

    }
    public TemplateInfo(Guid templateUuid, string templateName, AccessType templateAccessType, string templateFolderPath, string templateFileName, string templateTagsFileName)
    {
        TemplateUuid = templateUuid;
        TemplateName = templateName;
        TemplateAccessType = templateAccessType;
        TemplateFolderPath = templateFolderPath;
        TemplateFileName = templateFileName;
        TemplateTagsFileName = templateTagsFileName;
    }

    public bool Equals(TemplateInfo? other)
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

        return obj is TemplateInfo model && Equals(model);
    }
}