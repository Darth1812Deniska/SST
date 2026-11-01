using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia.Enums;
using SqlScriptToolAvalonia.Constants;
using SqlScriptToolAvalonia.Helpers;
using SSTCommonClasses.ConstructorStorage;
using SSTCommonClasses.Constants;
using SSTCommonClasses.Constructor.Models;
using SSTCommonClasses.Constructor.Tags;

namespace SqlScriptToolAvalonia.ConstructorWindows;

/// <summary>
/// Логика взаимодействия для SqlTemplateEditorWindow.axaml
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
        get => _sqlTemplateTags ?? new List<Tag>();
        set => _sqlTemplateTags = value;
    }

    private bool SqlTemplateIsMultiplePattern => ChbIsMultiplePattern.IsChecked ?? false;

    #endregion

    #region Getters-Setters

    private string GetSqlTemplateName()
    {
        return string.IsNullOrEmpty(_sqlTemplateName) ? TbName.Text?.Trim() ?? string.Empty : _sqlTemplateName;
    }

    private string? GetSqlTemplateDisplayLabel()
    {
        return string.IsNullOrEmpty(_sqlTemplateDisplayLabel)
            ? TbDisplayLabel.Text?.Trim()
            : _sqlTemplateDisplayLabel;
    }

    private string? GetSqlTemplateDescription()
    {
        return string.IsNullOrWhiteSpace(_sqlTemplateDescription)
            ? TbDescription.Text?.Trim()
            : _sqlTemplateDescription;
    }

    private DbType GetSqlTemplateDbType()
    {
        DbType result = _sqlTemplateDbType;
        if (RbPostgreSql.IsChecked == true)
        {
            result = DbType.PostgreSql;
        }

        if (RbMsSql.IsChecked == true)
        {
            result = DbType.MsSql;
        }

        return result;
    }

    private string? GetSqlTemplateText()
    {
        return string.IsNullOrEmpty(TbCodeEditor.Text?.Trim()) ? _sqlTemplateText : TbCodeEditor.Text?.Trim();
    }

    private SqlTemplate GetCurrentSqlTemplate()
    {
        // Перед формированием результата синхронизируем имена тэгов и дочерних
        // sql-шаблонов с текущим названием шаблона (требование: изменённый узел
        // должен называться так же, как шаблон, его содержащий).
        SyncNamesWithTemplate();

        if (InputSqlTemplate != null)
        {
            // Режим редактирования: возвращаем ТОТ ЖЕ самый объект шаблона, который
            // был передан в окно (с тем же Uuid). Это критично:
            // 1) имя узла на дереве модели корректно обновляется после закрытия окна;
            // 2) ссылка «тэг ModelItemParent -> дочерний sql-шаблон» остаётся прежним
            //    объектом, поэтому при повторном открытии шаблона не создаётся дубль
            //    тэга с тем же именем и не теряется привязка по uuid.
            InputSqlTemplate.AccessType = AccessType.Private;
            InputSqlTemplate.DbType = SqlTemplateDbType;
            InputSqlTemplate.Description = SqlTemplateDescription;
            InputSqlTemplate.DisplayLabel = SqlTemplateDisplayLabel;
            InputSqlTemplate.IsMultiplePattern = SqlTemplateIsMultiplePattern;
            InputSqlTemplate.Name = SqlTemplateName;
            InputSqlTemplate.SqlText = SqlTemplateText ?? InputSqlTemplate.SqlText;
            InputSqlTemplate.Tags = SqlTemplateTags;
            return InputSqlTemplate;
        }

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

    /// <summary>
    /// Синхронизация имени шаблона с именами тэга, под которым он используется
    /// в родительском шаблоне (и его текстом), и с именем дочернего sql-шаблона,
    /// прикреплённого к тэгу.
    /// Выполняется только если имя шаблона было изменено пользователем.
    /// </summary>
    private void SyncNamesWithTemplate()
    {
        if (InputSqlTemplate == null)
        {
            return;
        }

        string currentName = SqlTemplateName;
        string? oldName = InputSqlTemplate.Name;

        // Имя не менялось — ничего не делаем
        if (string.IsNullOrEmpty(oldName) || oldName == currentName)
        {
            return;
        }

        // 1. Переименовываем тэг, представляющий данный шаблон в родительских шаблонах,
        //    и заменяем его имя в текстах этих родительских шаблонов.
        //    Ссылки на связанные объекты (тэг -> этот шаблон) при этом НЕ трогаем:
        //    объект шаблона остаётся тем же (Uuid сохраняется), поэтому привязка
        //    по uuid не рвётся и дубль тэга при переоткрытии не создаётся.
        foreach (SqlTemplate parentCandidate in StaticConstructorStorage.SqlTemplates)
        {
            if (ReferenceEquals(parentCandidate, this) || ReferenceEquals(parentCandidate, InputSqlTemplate))
            {
                continue;
            }

            bool renamed = false;
            foreach (Tag parentTag in parentCandidate.Tags)
            {
                if (parentTag.TagType != TagType.ModelItemParent)
                {
                    continue;
                }

                // Тэг указывает на редактируемый шаблон: по ссылке на объект либо по uuid
                bool pointsToCurrentTemplate =
                    ReferenceEquals(parentTag.SqlTemplate, InputSqlTemplate)
                    || (parentTag.SqlTemplateUuid is Guid tagTemplateUuid && tagTemplateUuid == SqlTemplateUuid);

                if (pointsToCurrentTemplate && parentTag.Name == oldName)
                {
                    parentTag.SetName(currentName);
                    renamed = true;
                }
            }

            // Заменяем старое имя тэга на новое в тексте родительского шаблона
            if (renamed && !string.IsNullOrEmpty(parentCandidate.SqlText)
                          && parentCandidate.SqlText.Contains(oldName, StringComparison.Ordinal))
            {
                parentCandidate.SqlText = parentCandidate.SqlText.Replace(oldName, currentName);
            }
        }

        // 2. Если старый текст редактируемого шаблона содержал старый тэг, переименовываем
        //    его в текущем редакторе (с сохранением правок пользователя).
        //    Условие: новое имя шаблона ещё не присутствует в тексте — иначе пользователь
        //    сам уже вписал новый тэг и автоматическая замена создала бы дубль.
        string? oldSqlText = InputSqlTemplate.SqlText;
        string currentEditorText = TbCodeEditor.Text ?? string.Empty;
        if (!string.IsNullOrEmpty(oldName)
            && !currentEditorText.Contains(currentName, StringComparison.Ordinal))
        {
            int occurrences = string.IsNullOrEmpty(oldSqlText)
                ? 0
                : Regex.Matches(oldSqlText, Regex.Escape(oldName)).Count;
            if (occurrences >= 1 && currentEditorText.Contains(oldName, StringComparison.Ordinal))
            {
                TbCodeEditor.Text = currentEditorText.Replace(oldName, currentName);
            }
        }

        // 3. Тэги ModelItemParent самого редактируемого шаблона с унаследованными именами:
        //    sql-шаблон дочернего узла называется так же, как тэг, который его содержит.
        //    При переименовании родителя префикс имени заменяется на новое имя шаблона.
        foreach (Tag tag in SqlTemplateTags)
        {
            if (tag.TagType != TagType.ModelItemParent)
            {
                continue;
            }

            // Тэг, представляющий сам редактируемый шаблон в родителе, обработан выше
            if (tag.Name == oldName)
            {
                tag.SetName(currentName);
                continue;
            }

            if (tag.SqlTemplate != null && !string.IsNullOrEmpty(tag.Name)
                && oldName.Length > 2 && tag.Name.StartsWith(oldName[..^1], StringComparison.Ordinal))
            {
                string suffix = tag.Name[oldName.Length..]; // хвост вида "_ModelItem_N>"
                string childNewName = currentName + suffix;
                tag.SetName(childNewName);
                tag.SqlTemplate.SetName(childNewName);
            }
        }
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

        LbTemplateTags.ItemsSource = StaticConstructorStorage.TagTemplates.ToList();
    }

    private void InitializeNodeTags()
    {
        LbNodeTags.ItemsSource = SqlTemplateTags;
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

        if (!(RbMsSql.IsChecked == true || RbPostgreSql.IsChecked == true))
        {
            AvaloniaMessageBox.Show("Не выбран Тип СУБД");
            return false;
        }

        if (string.IsNullOrEmpty(TbCodeEditor.Text?.Trim()))
        {
            AvaloniaMessageBox.Show("Отсутствует код шаблона");
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
                AvaloniaMessageBox.ShowError(e.Message, "Ошибка при сохранении sql-шаблона");
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

    private async void BtnTagEdit_Click(object? sender, RoutedEventArgs e)
    {
        Tag? selTag = LbNodeTags.SelectedItem as Tag;
        if (selTag == null)
        {
            return;
        }

        TagEditorWindow tagEditor = new TagEditorWindow(selTag);
        await tagEditor.ShowDialog(this);

        // Обновляем список
        LbNodeTags.ItemsSource = null;
        SqlTemplateTags.Remove(selTag);
        SqlTemplateTags.Add(tagEditor.CurrentTag);

        LbNodeTags.ItemsSource = SqlTemplateTags;

        // Обновляем название тэга в тексте
        string? oldTagName = selTag.Name;
        string? newTagName = tagEditor.CurrentTag.Name;

        if (!string.IsNullOrEmpty(oldTagName))
        {
            TbCodeEditor.Text = TbCodeEditor.Text?.Replace(oldTagName, newTagName ?? string.Empty);
        }
    }

    private void LbNodeTags_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        SetTagDescriptionText();
    }

    private void TbCodeEditor_TextChanged(object? sender, TextChangedEventArgs e)
    {
        LbNodeTags.ItemsSource = null;
        if (!string.IsNullOrEmpty(SqlTemplateText))
        {
            // Выбираем все тэги в тексте
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
