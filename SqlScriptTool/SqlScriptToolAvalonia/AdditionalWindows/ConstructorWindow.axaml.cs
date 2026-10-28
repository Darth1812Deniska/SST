using System.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using SqlScriptToolAvalonia.ConstructorForms;
using SqlScriptToolClient.ConstructorStorage;
using SSTCommonClasses.Constructor.Models;
using SSTCommonClasses.Constructor.Tags;

namespace SqlScriptToolAvalonia.AdditionalWindows;

/// <summary>
/// Окно конструктора: кнопки создания новых объектов (моделей, sql-шаблонов, тэгов)
/// и списки уже созданных объектов для редактирования.
/// Основа - MainWindow проекта SqlScriptToolClient (без кнопки "Сгенерировать скрипт")
/// </summary>
public partial class ConstructorWindow : Window
{
    #region Constructor

    public ConstructorWindow()
    {
        InitializeComponent();
        InitModelsList();
        InitTemplatesList();
        InitTagsList();
    }

    #endregion

    #region Private methods

    private void InitModelsList()
    {
        InitList(SpModelsList, StaticConstructorStorage.Models);
    }

    private void InitTemplatesList()
    {
        InitList(SpTemplatesList, StaticConstructorStorage.SqlTemplates);
    }

    private void InitTagsList()
    {
        InitList(SpTagsList, StaticConstructorStorage.TagTemplates);
    }

    private void InitAllLists()
    {
        InitModelsList();
        InitTemplatesList();
        InitTagsList();
    }

    private void InitList(StackPanel stackPanel, IEnumerable list)
    {
        stackPanel.Children.Clear();
        foreach (object? item in list)
        {
            if (item != null)
            {
                Button btn = new Button()
                {
                    Content = item.ToString(),
                    Margin = new Avalonia.Thickness(2),
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                btn.Tag = item;
                btn.Click += Btn_OnClick;
                stackPanel.Children.Add(btn);
            }
        }
    }

    /// <summary>
    /// Открытие окна редактора и обновление соответствующего списка после его закрытия
    /// </summary>
    private void OpenEditorWindow(Window editorWindow)
    {
        editorWindow.Closed += (_, _) =>
        {
            // Обновляем все списки после закрытия окна редактора
            Dispatcher.UIThread.Post(InitAllLists);
        };
        editorWindow.Show();
    }

    #endregion

    #region Events

    /// <summary>
    /// Нажатие кнопки из списка существующих объектов - открытие окна на редактирование
    /// </summary>
    private void Btn_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button senderButton && senderButton.Tag is object obj)
        {
            switch (obj)
            {
                case Model model:
                    OpenEditorWindow(new ModelEditorWindow(model));
                    break;
                case SqlTemplate sqlTemplate:
                    OpenEditorWindow(new SqlTemplateEditorWindow(sqlTemplate));
                    break;
                case TagTemplate tagTemplate:
                    OpenEditorWindow(new TagEditorWindow(tagTemplate));
                    break;
            }
        }
    }

    /// <summary>
    /// Создание новой модели
    /// </summary>
    private void BtnCreateNewModel_OnClick(object? sender, RoutedEventArgs e)
    {
        OpenEditorWindow(new ModelEditorWindow());
    }

    /// <summary>
    /// Создание нового sql-шаблона
    /// </summary>
    private void BtnCreateNewSqlTemplate_OnClick(object? sender, RoutedEventArgs e)
    {
        OpenEditorWindow(new SqlTemplateEditorWindow());
    }

    /// <summary>
    /// Создание нового шаблона тэга
    /// </summary>
    private void BtnCreateNewTag_OnClick(object? sender, RoutedEventArgs e)
    {
        OpenEditorWindow(new TagEditorWindow());
    }

    private void MiSettings_OnClick(object? sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow();
        settingsWindow.Show();
    }

    #endregion
}
