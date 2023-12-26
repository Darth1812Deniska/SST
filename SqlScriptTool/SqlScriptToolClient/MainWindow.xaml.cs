using System;
using System.CodeDom;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using SqlScriptToolClient.ConstructorStorage;
using SSTCommonClasses.Models;
using SSTCommonClasses.Tags;

namespace SqlScriptToolClient;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        InitModelsList();
        InitTemplatesList();
        InitTagsList();
    }

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
        InitList(SpTagsList, StaticConstructorStorage.Tags);
    }

    private void InitList(StackPanel stackPanel, IEnumerable list)
    {
        foreach (var item in list)
        {
            if (item != null)
            {
                Button btn = new Button()
                {
                    Content = item,
                };
                btn.Click += Btn_Click;
                stackPanel.Children.Add(btn);
            }
        }
    }

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
                            ModelEditorWindow modelEditor = new ModelEditorWindow(model);
                            modelEditor.Show();
                        }

                        break;
                    case "SqlTemplate":
                        if (obj is SqlTemplate sqlTemplate)
                        {
                            SqlTemplateEditorWindow templateEditor = new SqlTemplateEditorWindow(sqlTemplate);
                            templateEditor.Show();
                        }

                        break;
                    case "Tag":
                        if (obj is Tag tag)
                        {
                            TagEditorWindow tagEditor = new TagEditorWindow(tag);
                            tagEditor.Show();
                        }

                        break;
                    
                }
            }
        }
    }

    private void BtnCreateNewModel_Click(object sender, RoutedEventArgs e)
    {
        ModelEditorWindow modelEditorWindow = new ModelEditorWindow();
        modelEditorWindow.Show();
    }

    private void BtnCreateNewSqlTemplate_Click(object sender, RoutedEventArgs e)
    {
        SqlTemplateEditorWindow sqlTemplateEditorWindow = new SqlTemplateEditorWindow();
        sqlTemplateEditorWindow.Show();
    }

    private void BtnCreateNewTag_Click(object sender, RoutedEventArgs e)
    {
        TagEditorWindow tagEditorWindow = new TagEditorWindow();
        tagEditorWindow.Show();
    }
}