using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using SqlScriptToolClient.ConstructorStorage;
using SSTCommonClasses.Constructor.Models;

namespace SqlScriptToolAvalonia.GeneratorWindows;

public partial class ModelGenerationListWindow : Window
{
    public ObservableCollection<Model> Models { get; set; } = new ObservableCollection<Model>();

    public ModelGenerationListWindow()
    {
        InitializeComponent();
        Models = new ObservableCollection<Model>(StaticConstructorStorage.Models);
        DgModels.ItemsSource = Models;
        Dictionary<string, string> modelFields = new()
        {
            { "DbType", "Тип СУБД" },
            { "Name", "Модель" },
            { "DisplayLabel", "Название" },
            { "Version", "Версия" },
            { "CreatorName", "Создатель" }
        };
        foreach (var field in modelFields)
        {
            string fieldName = field.Key;
            string columnHeader = field.Value;
            DataGridColumn newModelColumn = new DataGridTextColumn()
            {
                Binding = new Binding()
                {
                    Path = fieldName,
                    Mode = BindingMode.OneWay
                },
                Header = columnHeader
            };

            DgModels.Columns.Add(newModelColumn);
        }
        
        var buttonColumn = new DataGridTemplateColumn
        {
            Header = "Выбрать"
        };
        var columnTemplate = new FuncDataTemplate<Model>((model, recycled) =>
        {
            var button = new Button
            {
                Content = "Выбрать",
                Tag = model
            };
            
            var container = new StackPanel { Children = { button } };
            return container;
        });

        buttonColumn.CellTemplate = columnTemplate;

        DgModels.Columns.Add(buttonColumn);
    }

    private void RbDbType_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
}