using SqlScriptToolClient.ConstructorStorage;
using SSTCommonClasses.Constructor.Models;
using SSTCommonClasses.Constructor.Tags;
using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace SqlScriptToolClient;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    #region Constructor

    public MainWindow()
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
        foreach (var item in list)
        {
            if (item != null)
            {
                Button btn = new Button()
                {
                    Content = item,
                    Margin = new Thickness(2)
                };
                btn.Click += Btn_Click;
                stackPanel.Children.Add(btn);
            }
        }
    }

    #endregion

    #region Events

    private void Btn_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button senderButton)
        {
            var obj = senderButton.Content;
            if (obj != null)
            {
                Type type = obj.GetType();
                switch (type.Name)
                {
                    case "Model":
                        if (obj is Model model)
                        {
                            Visibility = Visibility.Collapsed;
                            ModelEditorWindow modelEditor = new ModelEditorWindow(model);
                            var dialogRes = modelEditor.ShowDialog();
                            if (dialogRes.HasValue && dialogRes.Value)
                            {
                                InitModelsList();
                                Visibility = Visibility.Visible;
                            }

                        }

                        break;
                    case "SqlTemplate":
                        if (obj is SqlTemplate sqlTemplate)
                        {
                            Visibility = Visibility.Collapsed;
                            SqlTemplateEditorWindow templateEditor = new SqlTemplateEditorWindow(sqlTemplate);
                            var dialogRes = templateEditor.ShowDialog();
                            if (dialogRes.HasValue && dialogRes.Value)
                            {
                                InitTemplatesList();
                                Visibility = Visibility.Visible;
                            }
                        }

                        break;
                    case "TagTemplate":
                        if (obj is TagTemplate tagTemplate)
                        {
                            Visibility = Visibility.Collapsed;
                            TagEditorWindow tagEditor = new TagEditorWindow(tagTemplate);
                            var dialogRes = tagEditor.ShowDialog();
                            if (dialogRes.HasValue && dialogRes.Value)
                            {
                                InitTagsList();
                                Visibility = Visibility.Visible;
                            }
                        }

                        break;

                }
            }
        }
    }

    private void BtnCreateNewModel_Click(object sender, RoutedEventArgs e)
    {
        Visibility = Visibility.Collapsed;
        ModelEditorWindow modelEditorWindow = new ModelEditorWindow();
        var dialogRes = modelEditorWindow.ShowDialog();
        if (dialogRes.HasValue && dialogRes.Value)
        {
            InitAllLists();
            Visibility = Visibility.Visible;
        }
    }

    private void BtnCreateNewSqlTemplate_Click(object sender, RoutedEventArgs e)
    {
        Visibility = Visibility.Collapsed;
        SqlTemplateEditorWindow sqlTemplateEditorWindow = new SqlTemplateEditorWindow();
        var dialogRes = sqlTemplateEditorWindow.ShowDialog();
        if (dialogRes.HasValue && dialogRes.Value)
        {
            InitAllLists();
            Visibility = Visibility.Visible;
        }
    }

    private void BtnCreateNewTag_Click(object sender, RoutedEventArgs e)
    {
        Visibility = Visibility.Collapsed;
        TagEditorWindow tagEditorWindow = new TagEditorWindow();
        var dialogRes = tagEditorWindow.ShowDialog();
        if (dialogRes.HasValue && dialogRes.Value)
        {
            InitAllLists();
            Visibility = Visibility.Visible;
        }
    }

    private void BtnSettings_Click(object sender, RoutedEventArgs e)
    {
        SettingsWindow settingsWindow = new SettingsWindow();
        settingsWindow.Show();
    }

    private void BtnGenScript_Click(object sender, RoutedEventArgs e)
    {
        ScriptGenerationSelectingModelsWindow window = new ScriptGenerationSelectingModelsWindow();
        window.Show();
    }

    #endregion
}