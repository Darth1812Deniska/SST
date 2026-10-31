using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using SSTCommonClasses.ConstructorStorage;
using SSTCommonClasses.Constants;
using SSTCommonClasses.Constructor.Models;

namespace SqlScriptToolAvalonia.GeneratorWindows;

public partial class ModelGenerationListWindow : Window
{
    public ObservableCollection<Model> Models { get; set; } = new ObservableCollection<Model>();

    /// <summary>
    /// Полный список моделей без фильтрации
    /// </summary>
    private readonly List<Model> _allModels = new();

    public ModelGenerationListWindow()
    {
        InitializeComponent();
        _allModels = StaticConstructorStorage.Models.ToList();
        Models = new ObservableCollection<Model>(_allModels);
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
            button.Click += BtnSelectModel_OnClick;

            var container = new StackPanel { Children = { button } };
            return container;
        });

        buttonColumn.CellTemplate = columnTemplate;

        DgModels.Columns.Add(buttonColumn);
    }

    private void RbDbType_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        // Фильтрация грида по выбранному типу СУБД (TagRadioButton'а: All / Ms / Pg).
        // Обработчик вызывается и при снятии отметки с предыдущего RadioButton'а группы,
        // поэтому берем Tag только того RadioButton'а, который сейчас отмечен.
        if (sender is not RadioButton radioButton || radioButton.IsChecked != true)
            return;

        string? filterTag = radioButton.Tag as string;

        IEnumerable<Model> filteredModels = filterTag switch
        {
            "Ms" => _allModels.Where(model => model.DbType == DbType.MsSql),
            "Pg" => _allModels.Where(model => model.DbType == DbType.PostgreSql),
            // "All" и любой неизвестный тег - показываем все модели
            _ => _allModels
        };

        Models.Clear();
        foreach (var model in filteredModels)
        {
            Models.Add(model);
        }
    }

    private async void BtnSelectModel_OnClick(object? sender, RoutedEventArgs e)
    {
        // Модель находится в Tag кнопки, расположенной в той же строке грида
        if (sender is Button { Tag: Model model })
        {
            var scriptGenParamsWindow = new ScriptGenerationParamsWindow(model);
            await scriptGenParamsWindow.ShowDialog(this);
        }
    }
}