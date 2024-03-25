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

namespace SqlScriptToolClient;

/// <summary>
/// Логика взаимодействия для SqlTemplateEditorWindow.xaml
/// </summary>
public partial class SqlTemplateEditorWindow : Window
{
    #region fields

    private string _sqlTemplateName;
    private string _sqlTemplateDisplayLabel;
    private string? _sqlTemplateDescription;
    private DbType _sqlTemplateDbType;
    private string _sqlTemplateText;

    #endregion

    #region Properties

    private TemplateEditorFormState FormState { get; }

    public SqlTemplate InputSqlTemplate { get; set; }
    public SqlTemplate CurrentSqlTemplate => GetCurrentSqlTemplate();

    private Guid SqlTemplateUuid { get; set; }

    private string SqlTemplateName
    {
        get => GetSqlTemplateName();
        set => _sqlTemplateName = value;
    }

    private string SqlTemplateDisplayLabel
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

    private string SqlTemplateText
    {
        get => GetSqlTemplateText();
        set => _sqlTemplateText = value;
    }

    private List<Tag> SqlTemplateTags { get; set; }
    private bool SqlTemplateIsMultiplePattern => ChbIsMultiplePattern.IsChecked != null && ChbIsMultiplePattern.IsChecked.Value;

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

    private string GetSqlTemplateText()
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

                break;
        }
    }

    private void InitializeNodeTags()
    {
        LbNodeTags.ItemsSource = SqlTemplateTags;
    }

    #endregion

    #region Events

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        StaticConstructorStorage.SaveSqlTemplate(CurrentSqlTemplate);
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void BtnTagEdit_Click(object sender, RoutedEventArgs e)
    {
        var selTag = LbNodeTags.SelectedItem as Tag;
        if (selTag == null)
        {
            return;
        }
        TagEditorWindow tagEditor = new TagEditorWindow(selTag);
        tagEditor.ShowDialog();

        LbNodeTags.ItemsSource = null;
        SqlTemplateTags.Remove(selTag);
        SqlTemplateTags.Add(tagEditor.CurrentTag);
        LbNodeTags.ItemsSource = SqlTemplateTags;
    }

    private void TbCodeEditor_TextChanged(object sender, TextChangedEventArgs e)
    {
        LbNodeTags.ItemsSource = null;
        if (!string.IsNullOrEmpty(SqlTemplateText))
        {
            // Выбираем все тэги в тектсе
            Regex rx = new Regex(@"<\w+>");
            var tagsInTextlist = rx.Matches(SqlTemplateText).Select(match => match.Value).Distinct().ToList();

            //Убираем существующие тэги, если их нет в тексте
            var tagsToRemove = SqlTemplateTags.Where(tag => !tagsInTextlist.Contains(tag.Name)).ToList();
            foreach (var tag in tagsToRemove)
            {
                SqlTemplateTags.Remove(tag);
            }

            // Выбираем только те строки тэгов, которых еще нет в списке тэгов
            var tagNames = SqlTemplateTags.Select(tag => tag.Name).ToList();
            var newTagNames = tagsInTextlist.Where(tagName => !tagNames.Contains(tagName)).ToList();

            foreach (string newTagName in newTagNames)
            {
                Tag newTag = new Tag(newTagName);
                SqlTemplateTags.Add(newTag);
            }
        }

        LbNodeTags.ItemsSource = SqlTemplateTags;
    }

    #endregion
}