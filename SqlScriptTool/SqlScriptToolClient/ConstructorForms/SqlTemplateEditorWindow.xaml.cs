using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using SqlScriptToolClient.Constants;
using SqlScriptToolClient.ConstructorStorage;
using SSTCommonClasses.Constants;
using SSTCommonClasses.Constructor.Models;
using SSTCommonClasses.Constructor.Tags;
using TextChangedEventArgs = System.Windows.Controls.TextChangedEventArgs;

namespace SqlScriptToolClient.ConstructorForms;

/// <summary>
/// Логика взаимодействия для SqlTemplateEditorWindow.xaml
/// </summary>
public partial class SqlTemplateEditorWindow : Window
{
    #region fields

    private string? _sqlTemplateName;
    private string? _sqlTemplateDisplayLabel;
    private string? _sqlTemplateDescription;
    private DbType _sqlTemplateDbType;
    private string? _sqlTemplateText;
    private bool _isSaveAndCloseClicked = false;
    private List<Tag>? _sqlTemplateTags = null;

    #endregion

    #region Properties

    private TemplateEditorFormState FormState { get; }

    public SqlTemplate? InputSqlTemplate { get; set; }
    public SqlTemplate CurrentSqlTemplate => GetCurrentSqlTemplate();

    private Guid SqlTemplateUuid { get; set; }

    private string SqlTemplateName
    {
        get => GetSqlTemplateName();
        set => _sqlTemplateName = value;
    }

    private string? SqlTemplateDisplayLabel
    {
        get => GetSqlTemplateDisplayLabel();
        set => _sqlTemplateDisplayLabel = value;
    }

    private string? SqlTemplateDescription
    {
        get => GetSqlTemplateDescription();
        set => _sqlTemplateDescription = value;
    }

    private DbType SqlTemplateDbType
    {
        get => GetSqlTemplateDbType();
        set => _sqlTemplateDbType = value;
    }

    private string? SqlTemplateText
    {
        get => GetSqlTemplateText();
        set => _sqlTemplateText = value;
    }

    private List<Tag> SqlTemplateTags
    {
        get => _sqlTemplateTags??new List<Tag>();
        set => _sqlTemplateTags = value;
    }

    private bool SqlTemplateIsMultiplePattern =>
        ChbIsMultiplePattern.IsChecked != null && ChbIsMultiplePattern.IsChecked.Value;

    #endregion

    #region Getters-Setters

    private string GetSqlTemplateName()
    {
        return string.IsNullOrEmpty(_sqlTemplateName) ? TbName.Text.Trim() : _sqlTemplateName;
    }

    private string GetSqlTemplateDisplayLabel()
    {
        return string.IsNullOrEmpty(_sqlTemplateDisplayLabel)
            ? TbDisplayLabel.Text.Trim()
            : _sqlTemplateDisplayLabel;
    }

    private string GetSqlTemplateDescription()
    {
        return string.IsNullOrWhiteSpace(_sqlTemplateDescription)
            ? TbDescription.Text.Trim()
            : _sqlTemplateDescription;
    }

    private DbType GetSqlTemplateDbType()
    {
        DbType result = _sqlTemplateDbType;
        if (RbPostgreSql.IsChecked != null && RbPostgreSql.IsChecked.Value)
        {
            result = DbType.PostgreSql;
        }

        if (RbMsSql.IsChecked != null && RbMsSql.IsChecked.Value)
        {
            result = DbType.MsSql;
        }

        return result;
    }

    private string? GetSqlTemplateText()
    {
        return string.IsNullOrEmpty(TbCodeEditor.Text.Trim()) ? _sqlTemplateText : TbCodeEditor.Text.Trim();
    }

    private SqlTemplate GetCurrentSqlTemplate()
    {
        SqlTemplate sqlTemplate = new SqlTemplate()
        {
            AccessType = AccessType.Private,
            DbType = SqlTemplateDbType,
            Creator = null,
            Description = SqlTemplateDescription,
            DisplayLabel = SqlTemplateDisplayLabel,
            IsMultiplePattern = SqlTemplateIsMultiplePattern,
            Name = SqlTemplateName,
            SqlText = SqlTemplateText,
            Tags = SqlTemplateTags,
            Uuid = SqlTemplateUuid
        };

        return sqlTemplate;
    }

    #endregion

    #region Constructors

    public SqlTemplateEditorWindow()
    {
        FormState = TemplateEditorFormState.Creation;
        InitializeComponent();
        InitializeProperties();
    }

    public SqlTemplateEditorWindow(SqlTemplate sqlTemplate)
    {
        FormState = TemplateEditorFormState.Editing;
        InputSqlTemplate = sqlTemplate;
        InitializeComponent();
        InitializeProperties();
    }

    #endregion

    #region private methods

    private void InitializeProperties()
    {
        switch (FormState)
        {
            case TemplateEditorFormState.Creation:
                SqlTemplateUuid = Guid.NewGuid();
                TbUuid.Text = SqlTemplateUuid.ToString();
                SqlTemplateTags = new List<Tag>();
                break;
            case TemplateEditorFormState.Editing:
                if (InputSqlTemplate != null)
                {
                    SqlTemplateUuid = InputSqlTemplate.Uuid;
                    SqlTemplateName = InputSqlTemplate.Name;
                    SqlTemplateDisplayLabel = InputSqlTemplate.DisplayLabel;
                    SqlTemplateDescription = InputSqlTemplate.Description;
                    SqlTemplateDbType = InputSqlTemplate.DbType;
                    SqlTemplateText = InputSqlTemplate.SqlText;
                    SqlTemplateTags = InputSqlTemplate.Tags;

                    TbName.Text = SqlTemplateName;
                    TbDisplayLabel.Text = SqlTemplateDisplayLabel;
                    TbDescription.Text = SqlTemplateDescription;
                    TbCodeEditor.Text = SqlTemplateText;
                    TbUuid.Text = SqlTemplateUuid.ToString();
                    ChbIsMultiplePattern.IsChecked = InputSqlTemplate.IsMultiplePattern;

                    switch (SqlTemplateDbType)
                    {
                        case DbType.PostgreSql:
                            RbPostgreSql.IsChecked = true;
                            break;
                        case DbType.MsSql:
                            RbMsSql.IsChecked = true;
                            break;
                    }

                    InitializeNodeTags();
                }

                break;
        }

        LbTemplateTags.ItemsSource = StaticConstructorStorage.TagTemplates;
    }

    private void InitializeNodeTags()
    {
        LbNodeTags.ItemsSource = SqlTemplateTags;
    }

    private bool CheckFields()
    {
        if (string.IsNullOrEmpty(TbName.Text.Trim()))
        {
            MessageBox.Show("Не заполнено поле \"Название\"");
            return false;
        }

        if (string.IsNullOrEmpty(TbDisplayLabel.Text.Trim()))
        {
            MessageBox.Show("Не заполнено поле \"Отображаемое наименование\"");
            return false;
        }

        if (!(RbMsSql.IsChecked.GetValueOrDefault() || RbPostgreSql.IsChecked.GetValueOrDefault()))
        {
            MessageBox.Show("Не выбран Тип СУБД");
            return false;
        }

        if (string.IsNullOrEmpty(TbCodeEditor.Text.Trim()))
        {
            MessageBox.Show("Отсутствует код шаблона");
            return false;
        }

        return true;
    }

    private bool Save()
    {
        if (CheckFields())
        {
            try
            {
                StaticConstructorStorage.SaveSqlTemplate(CurrentSqlTemplate);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Ошибка: {e.Message}", "Ошибка при сохранении sql-шаблона", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        return false;
    }

    private void SetTagDescriptionText()
    {
        if (LbNodeTags.SelectedItem is Tag tag)
        {
            string result =
                $"{tag.Name}{(string.IsNullOrEmpty(tag.DisplayLabel) ? string.Empty : $" ({tag.DisplayLabel})")}:\n" +
                $"Тип тэга: {tag.TagType};\n" +
                $"Тип данных пользовательского параметра: {tag.DataType};\n" +
                $"{(tag.SqlTemplate == null ? string.Empty : $"SQL-шаблон: {tag.SqlTemplate}")}";


            TbTagPreview.Text = result;
        }
    }

    #endregion

    #region Events

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        Save();
    }

    private void BtnSaveAndClose_Click(object sender, RoutedEventArgs e)
    {
        if (Save())
        {
            _isSaveAndCloseClicked = true;
            Close();
        }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void BtnTagEdit_Click(object sender, RoutedEventArgs e)
    {
        Tag? selTag = LbNodeTags.SelectedItem as Tag;
        if (selTag == null)
        {
            return;
        }

        TagEditorWindow tagEditor = new TagEditorWindow(selTag);
        tagEditor.ShowDialog();

        // Обновляем список 
        LbNodeTags.ItemsSource = null;
        SqlTemplateTags.Remove(selTag);
        SqlTemplateTags.Add(tagEditor.CurrentTag);

        LbNodeTags.ItemsSource = SqlTemplateTags;

        // Обновляем название тэга в тексте
        string? oldTagName = selTag.Name;
        string? newTagName = tagEditor.CurrentTag.Name;

        TbCodeEditor.Text = TbCodeEditor.Text.Replace(oldTagName, newTagName);
    }

    private void LbNodeTags_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        SetTagDescriptionText();
    }

    private void TbCodeEditor_TextChanged(object sender, TextChangedEventArgs e)
    {
        LbNodeTags.ItemsSource = null;
        if (!string.IsNullOrEmpty(SqlTemplateText))
        {
            // Выбираем все тэги в тектсе
            Regex rx = new Regex(@"<\w+>");
            List<string> tagsInTextlist = rx.Matches(SqlTemplateText).Select(match => match.Value).Distinct().ToList();

            //Убираем существующие тэги, если их нет в тексте
            List<Tag> tagsToRemove = SqlTemplateTags.Where(tag => !tagsInTextlist.Contains(tag.Name)).ToList();
            foreach (Tag tag in tagsToRemove)
            {
                SqlTemplateTags.Remove(tag);
            }

            // Выбираем только те строки тэгов, которых еще нет в списке тэгов
            List<string> tagNames = SqlTemplateTags.Select(tag => tag.Name).ToList();
            List<string> newTagNames = tagsInTextlist.Where(tagName => !tagNames.Contains(tagName)).ToList();

            foreach (string? newTagName in newTagNames)
            {
                Tag newTag = new Tag(newTagName);
                SqlTemplateTags.Add(newTag);
            }
        }

        LbNodeTags.ItemsSource = SqlTemplateTags;
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_isSaveAndCloseClicked)
        {
            DialogResult = true;
            return;
        }

        MessageBoxResult result = MessageBox.Show("Закрыть окно? Несохраненные изменения будут отменены!", "Закрыть окно?",
            MessageBoxButton.OKCancel, MessageBoxImage.Question);
        if (result == MessageBoxResult.Cancel)
        {
            e.Cancel = true;
        }

        DialogResult = true;
    }

    #endregion
}