using System;
using System.Linq;
using System.Windows;
using SqlScriptToolClient.Constants;
using SqlScriptToolClient.ConstructorStorage;
using SSTCommonClasses.Constants;
using SSTCommonClasses.Constructor.Models;
using SSTCommonClasses.Constructor.Tags;
using SSTCommonClasses.Landocs;

namespace SqlScriptToolClient;

/// <summary>
/// Логика взаимодействия для TagEditorWindow.xaml
/// </summary>
public partial class TagEditorWindow : Window
{
    #region Fields

    private bool _isSaveAndCloseClicked = false;

    #endregion

    #region Properties

    private TagEditorFormState FormState { get; }
    private Tag? SelectedTag { get; }
    private TagTemplate? SelectedTagTemplate { get; }
    public Tag CurrentTag => GetCurrentTag();
    public TagTemplate CurrentTagTemplate => GetCurrentTagTemplate();


    private Guid TagUuid { get; set; }

    private string TagName => GetTagName();

    private string? TagDescription => GetTagDescription();

    private string TagDisplayLabel => GetTagDisplayLabel();

    private TagType TagType => GetTagType();

    private TagDataType? TagDataType => GetTagDataType();

    private LdObjectType? LdObjectType => GetLdObjectType();

    private SqlTemplate? SqlTemplate => GetSqlTemplate();
    private bool IsMultiplePattern => GetIsMultiplePattern();


    #endregion

    #region Getters - setters

    private string GetTagName()
    {
        return TbName.Text.Trim();
    }

    private string GetTagDescription()
    {
        return TbDescription.Text.Trim();
    }

    private string GetTagDisplayLabel()
    {
        return TbDisplayLabel.Text.Trim();
    }


    private TagType GetTagType()
    {
        if (RbCalculatedValue.IsChecked != null && RbCalculatedValue.IsChecked.Value)
        {
            return TagType.CalculatedValue;
        }

        if (RbConstant.IsChecked != null && RbConstant.IsChecked.Value)
        {
            return TagType.Constant;
        }

        if (RbModelItemParent.IsChecked != null && RbModelItemParent.IsChecked.Value)
        {
            return TagType.ModelItemParent;
        }

        if (RbUserParameter.IsChecked != null && RbUserParameter.IsChecked.Value)
        {
            return TagType.UserParameter;
        }

        return TagType.None;
    }

    private TagDataType? GetTagDataType()
    {
        if (CmbDataType.SelectedItem == null)
        {
            return null;
        }

        return CmbDataType.SelectedItem as TagDataType?;
    }

    private LdObjectType? GetLdObjectType()
    {
        if (CmbLdObjectType.SelectedItem == null)
        {
            return null;
        }

        return CmbLdObjectType.SelectedItem as LdObjectType;
    }

    private SqlTemplate? GetSqlTemplate()
    {
        if (CmbSqlTemplate.SelectedItem == null)
        {
            return null;
        }

        return CmbSqlTemplate.SelectedItem as SqlTemplate;
    }

    private bool GetIsMultiplePattern()
    {
        return CbMultiplePattern.IsChecked.HasValue && CbMultiplePattern.IsChecked.Value;
    }

    /*private SqlTemplate? GetSqlTemplate()
    {
        if (SelectedTag == null)
        {
            return null;
        }

        _childModelItem = SelectedTag.SqlTemplate;
        if (_childModelItem != null)
        {
            _childModelItem.SqlTemplate = SqlTemplate;
            return _childModelItem;
        }

        return null;
    }*/

    private Tag GetCurrentTag()
    {
        Tag result = new Tag()
        {
            AccessType = AccessType.Private,
            DataType = TagDataType,
            Description = TagDescription,
            DisplayLabel = TagDisplayLabel,
            LdObjectType = LdObjectType,
            Name = TagName,
            SqlTemplate = SqlTemplate,
            TagType = TagType,
            Uuid = TagUuid,
            IsMultiplePattern = IsMultiplePattern
        };
        return result;
    }

    private TagTemplate GetCurrentTagTemplate()
    {
        TagTemplate result = new TagTemplate()
        {
            Uuid = TagUuid,
            Name = TagName,
            DisplayLabel = TagDisplayLabel,
            Description = TagDescription,
            TagType = TagType,
            DataType = TagDataType,
            LdObjectType = LdObjectType,
            SqlTemplate = SqlTemplate,
            IsMultiplePattern = IsMultiplePattern
        };
        return result;
    }

    #endregion

    #region Constructors

    public TagEditorWindow()
    {
        FormState = TagEditorFormState.TemplateCreation;
        InitializeComponent();
        InitializeProperties();
    }

    public TagEditorWindow(Tag tag)
    {
        SelectedTag = tag;
        FormState = TagEditorFormState.TagEditing;
        InitializeComponent();
        InitializeProperties();
    }

    public TagEditorWindow(TagTemplate tagTemplate)
    {
        SelectedTagTemplate = tagTemplate;
        FormState = TagEditorFormState.TemplateEditing;
        InitializeComponent();
        InitializeProperties();
    }

    #endregion

    #region Private methods

    private void InitializeProperties()
    {
        // Присваиваем просто значения без отображаемых наименований
        CmbDataType.ItemsSource = Enum.GetValues(typeof(TagDataType)).Cast<TagDataType>();
        // Присваиваем пока все sql-шаблоны
        CmbSqlTemplate.ItemsSource = StaticConstructorStorage.SqlTemplates;

        switch (FormState)
        {
            case TagEditorFormState.TemplateCreation:
                TagUuid = Guid.NewGuid();
                TbUuid.Text = TagUuid.ToString();
                break;
            case TagEditorFormState.TagEditing:
                if (SelectedTag != null)
                {
                    TagUuid = SelectedTag.Uuid;
                    TbUuid.Text = TagUuid.ToString();
                    TbName.Text = SelectedTag.Name;
                    TbDisplayLabel.Text = SelectedTag.DisplayLabel;
                    TbDescription.Text = SelectedTag.Description;
                    CbMultiplePattern.IsChecked = SelectedTag.IsMultiplePattern;
                    switch (SelectedTag.TagType)
                    {
                        case TagType.ModelItemParent:
                            RbModelItemParent.IsChecked = true;
                            break;
                        case TagType.CalculatedValue:
                            RbCalculatedValue.IsChecked = true;
                            break;
                        case TagType.Constant:
                            RbConstant.IsChecked = true;
                            break;
                        case TagType.MultiplePattern:
                            CbMultiplePattern.IsChecked = true;
                            break;
                        case TagType.UserParameter:
                            RbUserParameter.IsChecked = true;
                            break;
                    }

                    if (SelectedTag.DataType != null)
                    {
                        CmbDataType.SelectedItem = SelectedTag.DataType;
                    }

                    // пока еще не закружались Типы объектов
                    if (SelectedTag.LdObjectType != null)
                    {
                        CmbLdObjectType.SelectedItem = SelectedTag.LdObjectType;
                    }

                    if (SelectedTag.SqlTemplate != null)
                    {
                        CmbSqlTemplate.SelectedItem = SelectedTag.SqlTemplate;
                    }
                }

                break;
        }
    }

    private void Save()
    {
        switch (FormState)
        {
            case TagEditorFormState.TemplateCreation:
            case TagEditorFormState.TemplateEditing:
                SaveTemplate();
                break;
            case
                TagEditorFormState.TagEditing:
                SaveTag();
                break;
        }
    }

    private void SaveTemplate()
    {
        StaticConstructorStorage.TagTemplates.AddOrReplace(CurrentTagTemplate);
    }

    private void SaveTag()
    {
        StaticConstructorStorage.Tags.AddOrReplace(CurrentTag);
    }

    #endregion

    #region Events

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        Save();
    }

    private void BtnSaveAndClose_Click(object sender, RoutedEventArgs e)
    {
        Save();
        _isSaveAndCloseClicked = true;
        Close();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void cbMultiplePattern_Checked(object sender, RoutedEventArgs e)
    {
        if (CbMultiplePattern.IsChecked == true)
        {
            RbModelItemParent.IsChecked = true;
            RbUserParameter.IsEnabled = false;
            RbCalculatedValue.IsEnabled = false;
            RbConstant.IsEnabled = false;
        }
        else
        {
            RbModelItemParent.IsChecked = false;
            RbUserParameter.IsEnabled = true;
            RbCalculatedValue.IsEnabled = true;
            RbConstant.IsEnabled = true;
        }
    }

    private void WindowTagEditor_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_isSaveAndCloseClicked)
        {
            return;
        }

        var result = MessageBox.Show("Закрыть окно? Несохраненные изменения будут отменены!", "Закрыть окно?",
            MessageBoxButton.OKCancel, MessageBoxImage.Question);
        if (result == MessageBoxResult.Cancel)
        {
            e.Cancel = true;
        }
    }

    #endregion
}