using SSTCommonClasses.Constants;
using System;

namespace SqlScriptToolClient.ConstructorStorage.ObjectInfos;

public class TagTemplateInfo
{
    public Guid? TagTemplateUuid { get; }
    public string? TagTemplateName { get; set; }
    public AccessType? TagTemplateAccessType { get; set; }
    public string? TagTemplatePath { get; set; }
    public string? TagTemplateFileName { get; set; }

    public TagTemplateInfo()
    {
    }

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