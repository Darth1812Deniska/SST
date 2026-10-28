using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia.Enums;
using SqlScriptToolAvalonia.Constants;
using SqlScriptToolAvalonia.Helpers;
using SqlScriptToolClient.ConstructorStorage;
using SSTCommonClasses.Constants;
using SSTCommonClasses.Constructor.Models;
using SSTCommonClasses.Constructor.Tags;

namespace SqlScriptToolAvalonia.ConstructorWindows;

/// <summary>
/// Логика взаимодействия для TagEditorWindow.axaml
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

    private SqlTemplate? SqlTemplate => GetSqlTemplate();
    private bool IsMultiplePattern => GetIsMultiplePattern();

    #endregion

    #region Getters - setters

    private string GetTagName()
    {
        return TbName.Text?.Trim() ?? string.Empty;
    }

    private string GetTagDescription()
    {
        return TbDescription.Text?.Trim() ?? string.Empty;
    }

    private string GetTagDisplayLabel()
    {
        return TbDisplayLabel.Text?.Trim() ?? string.Empty;
    }


    private TagType GetTagType()
    {
        if (RbCalculatedValue.IsChecked == true)
        {
            return TagType.CalculatedValue;
        }

        if (RbConstant.IsChecked == true)
        {
            return TagType.Constant;
        }

        if (RbModelItemParent.IsChecked == true)
        {
            return TagType.ModelItemParent;
        }

        if (RbUserParameter.IsChecked == true)
        {
            return TagType.UserParameter;
        }

        return TagType.None;
    }

    private TagDataType? GetTagDataType()
    {
        return CmbDataType.SelectedItem as TagDataType?;
    }

    private SqlTemplate? GetSqlTemplate()
    {
        return CmbSqlTemplate.SelectedItem as SqlTemplate;
    }

    private bool GetIsMultiplePattern()
    {
        return CbMultiplePattern.IsChecked == true;
    }

    private Tag GetCurrentTag()
    {
        Tag result = new Tag()
        {
            AccessType = AccessType.Private,
            DataType = TagDataType,
            Description = TagDescription,
            DisplayLabel = TagDisplayLabel,
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
            SqlTemplate = SqlTemplate,
            IsMultiplePattern = IsMultiplePattern,
            AccessType = AccessType.Private
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
        CmbSqlTemplate.ItemsSource = StaticConstructorStorage.SqlTemplates.ToList();

        switch (FormState)
        {
            case TagEditorFormState.TemplateCreation:
                Title = "Редактор шаблонов тэгов";
                TagUuid = Guid.NewGuid();
                TbUuid.Text = TagUuid.ToString();
                break;
            case TagEditorFormState.TemplateEditing:
                Title = "Редактор шаблонов тэгов";
                if (SelectedTagTemplate != null)
                {
                    TagUuid = SelectedTagTemplate.Uuid;
                    TbName.Text = SelectedTagTemplate.Name;
                    TbDisplayLabel.Text = SelectedTagTemplate.DisplayLabel;
                    TbDescription.Text = SelectedTagTemplate.Description;
                    TbUuid.Text = TagUuid.ToString();
                    CbMultiplePattern.IsChecked = SelectedTagTemplate.IsMultiplePattern;
                    switch (SelectedTagTemplate.TagType)
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

                    if (SelectedTagTemplate.DataType != null)
                    {
                        CmbDataType.SelectedItem = SelectedTagTemplate.DataType;
                    }

                    if (SelectedTagTemplate.SqlTemplate != null)
                    {
                        CmbSqlTemplate.SelectedItem = SelectedTagTemplate.SqlTemplate;
                    }
                }

                break;
            case TagEditorFormState.TagEditing:
                Title = "Редактор тэгов";
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

                    if (SelectedTag.SqlTemplate != null)
                    {
                        CmbSqlTemplate.SelectedItem = SelectedTag.SqlTemplate;
                    }
                }

                break;
        }
    }

    private bool CheckFields()
    {
        if (string.IsNullOrEmpty(TbName.Text?.Trim()))
        {
            AvaloniaMessageBox.Show("Не заполнено поле \"Название\"");
            return false;
        }

        if (string.IsNullOrEmpty(TbDisplayLabel.Text?.Trim()))
        {
            AvaloniaMessageBox.Show("Не заполнено поле \"Отображаемое наименование\"");
            return false;
        }

        if (!(RbUserParameter.IsChecked == true ||
              RbModelItemParent.IsChecked == true ||
              RbCalculatedValue.IsChecked == true ||
              RbConstant.IsChecked == true
            ))
        {
            AvaloniaMessageBox.Show("Не выбран Тип СУБД");
            return false;
        }

        return true;
    }

    private bool Save()
    {
        if (CheckFields())
        {
            switch (FormState)
            {
                case TagEditorFormState.TemplateCreation:
                case TagEditorFormState.TemplateEditing:
                    return SaveTemplate();

                case TagEditorFormState.TagEditing:
                    return SaveTag();

            }
        }

        return false;
    }

    private bool SaveTemplate()
    {
        try
        {
            StaticConstructorStorage.SaveTagTemplate(CurrentTagTemplate);
        }
        catch (Exception e)
        {
            AvaloniaMessageBox.ShowError($"Ошибка: {e.Message}", "Ошибка при сохранении шаблона тэга");
            return false;
        }

        return true;
    }

    private bool SaveTag()
    {
        try
        {
            //StaticConstructorStorage.Tags.AddOrReplace(CurrentTag);
        }
        catch (Exception e)
        {
            AvaloniaMessageBox.ShowError($"Ошибка: {e.Message}", "Ошибка при сохранении  тэга");
            return false;
        }

        return true;
    }

    #endregion

    #region Events

    private void BtnSave_Click(object? sender, RoutedEventArgs e)
    {
        Save();
    }

    private void BtnSaveAndClose_Click(object? sender, RoutedEventArgs e)
    {
        if (Save())
        {
            _isSaveAndCloseClicked = true;
            Close();
        }
    }

    private void BtnCancel_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void BtnOpenCreate_Click(object? sender, RoutedEventArgs e)
    {
        object? selectedItem = CmbSqlTemplate.SelectionBoxItem;
        if (selectedItem == null)
        {
            SqlTemplateEditorWindow window = new SqlTemplateEditorWindow();
            await window.ShowDialog(this);
            return;
        }

        if (selectedItem is SqlTemplate sqlTemplate)
        {
            SqlTemplateEditorWindow window = new SqlTemplateEditorWindow(sqlTemplate);
            await window.ShowDialog(this);
        }
        else
        {
            SqlTemplateEditorWindow window = new SqlTemplateEditorWindow();
            await window.ShowDialog(this);
        }
    }

    private void cbMultiplePattern_Checked(object? sender, RoutedEventArgs e)
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

    private void CmbSqlTemplate_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        RbModelItemParent.IsChecked = true;
        if (CmbSqlTemplate.SelectedItem is SqlTemplate sqlTemplate)
        {
            string? newTagName = sqlTemplate.Name;
            if (!string.IsNullOrEmpty(newTagName))
            {
                TbName.Text = newTagName.Trim();
            }
        }
    }

    /// <summary>
    /// Обработка попытки закрытия окна (аналог WPF Closing с отменой)
    /// </summary>
    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);

        if (_isSaveAndCloseClicked)
        {
            return;
        }

        ButtonResult result = await AvaloniaMessageBox.ShowQuestionAsync(this,
            "Закрыть окно? Несохраненные изменения будут отменены!", "Закрыть окно?", ButtonEnum.OkCancel);
        if (result != ButtonResult.Ok)
        {
            e.Cancel = true;
        }
    }

    #endregion
}
