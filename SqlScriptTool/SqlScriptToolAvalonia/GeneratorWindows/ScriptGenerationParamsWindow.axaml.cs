using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Avalonia;
using Avalonia.Interactivity;

using SqlScriptToolAvalonia.CustomComponents;
using SqlScriptToolAvalonia.ScriptGeneration;

using SSTCommonClasses.Constructor;
using SSTCommonClasses.Constructor.Models;
using SSTCommonClasses.Constructor.Tags;


namespace SqlScriptToolAvalonia.GeneratorWindows;

/// <summary>
/// Логика взаимодействия для ScriptGenerationParamsWindow.xaml
/// </summary>
public partial class ScriptGenerationParamsWindow : Window
{
    #region properties

    private Model SelectedModel { get; }
    private ModelItem RootModelItem => SelectedModel.RootModelItem;
    private List<ConstructorItem> UserParamItems => GetUserParamItems();
    private Dictionary<Guid, int> UserParamControls { get; }
    private Dictionary<string, string> UserParamValues => GetUserParamValues();
    private string? ModelSqlTemplateText => ScriptGenerationStaticMethods.GetTemplateByModel(SelectedModel);

    #endregion


    #region Constructor

    public ScriptGenerationParamsWindow(Model model)
    {
        InitializeComponent();
        SelectedModel = model;
        UserParamControls = new Dictionary<Guid, int>();
        DrawUserParams();
    }

    #endregion

    #region Getters-Setters

    private List<ConstructorItem> GetUserParamItems()
    {
        return GetModelItemConItemsRec(RootModelItem);
    }

    private Dictionary<string, string> GetUserParamValues()
    {
        Dictionary<string, string> result = new Dictionary<string, string>();

        foreach (KeyValuePair<Guid, int> userParam in UserParamControls)
        {
            if (GrParamsList.Children[userParam.Value] is Control control)
            {
                KeyValuePair<string, string>? val = GetControlValue(control);
                if (val.HasValue)
                {
                    KeyValuePair<string, string> keyValue = val.Value;
                    result.Add(keyValue.Key, keyValue.Value);
                }
            }
        }

        return result;
    }

    #endregion

    #region private methods

    private void DrawUserParams()
    {
        int rowCount = 0;
        foreach (ConstructorItem userParamItem in UserParamItems)
        {
            Guid controlGuid = userParamItem.Uuid;

            RowDefinition rd = new RowDefinition()
            {
                Height = GridLength.Auto
            };
            GrParamsList.RowDefinitions.Add(rd);


            string constuctorItemType = userParamItem.GetType().Name;

            switch (constuctorItemType)
            {
                case "Tag":
                    if (userParamItem is Tag userParamTag)
                    {
                        string? displayLabel = string.IsNullOrEmpty(userParamTag.DisplayLabel)
                            ? userParamTag.Name
                            : userParamTag.DisplayLabel;
                        Label label = new Label();
                        {
                            Content = displayLabel;
                        };

                        Control? control = FieldControlByDataType(userParamTag);
                        if (control == null)
                        {
                            continue;
                        }

                        control.Margin = new Thickness(2);

                        // Добавляем лейбл
                        GrParamsList.Children.Add(label);
                        Grid.SetColumn(label, 0);
                        Grid.SetRow(label, rowCount);

                        // Добавляем поле для значений
                        //int controlIndex = GrParamsList.Children.Add(control);
                        //UserParamControls.Add(controlGuid, controlIndex);
                        Grid.SetColumn(control, 1);
                        Grid.SetRow(control, rowCount);
                    }
                    else
                    {
                        continue;
                    }

                    break;
                case "SqlTemplate":
                    if (userParamItem is SqlTemplate multipleParam)
                    {
                        // Создаем множественный контрол
                        MultiplePatternControl multiplePatternControl = new MultiplePatternControl(multipleParam);
                        // Добавляем к нему тэг
                        multiplePatternControl.Tag = multipleParam;
                        // Добавляем на грид
                        //int controlIndex = GrParamsList.Children.Add(multiplePatternControl);
                        // Привязываем к сетке
                        Grid.SetColumn(multiplePatternControl, 0);
                        Grid.SetRow(multiplePatternControl, rowCount);
                        Grid.SetColumnSpan(multiplePatternControl, 3);
                        // сохраняем индекс контрола
                        //UserParamControls.Add(controlGuid, controlIndex);
                    }
                    else
                    {
                        continue;
                    }

                    break;
                default:
                    continue;
            }

            rowCount++;
        }
    }

    private List<ConstructorItem> GetModelItemConItemsRec(ModelItem modelItem)
    {
        List<ConstructorItem> result = new List<ConstructorItem>();
        Dictionary<string, ModelItem> childModelItems = modelItem.ChildModelItems;
        SqlTemplate modelItemSqlTemplate = modelItem.SqlTemplate;

// Проверка Шаблона на множественность
        if (modelItemSqlTemplate.IsMultiplePattern)
        {
            result.Add(modelItemSqlTemplate);
            return result;
        }

        List<Tag> iterUserParamTags = modelItemSqlTemplate.Tags
            .Where(tag => tag.TagType == TagType.UserParameter)
            .ToList();
        result.AddRange(iterUserParamTags);
        foreach (KeyValuePair<string, ModelItem> childModelItemPair in childModelItems)
        {
            ModelItem childModelItem = childModelItemPair.Value;
            result.AddRange(GetModelItemConItemsRec(childModelItem));
        }

        return result;
    }

    private Control? FieldControlByDataType(Tag tag)
    {
        return ScriptGenControlsStaticMethods.FieldControlByDataType(tag);
    }

    private KeyValuePair<string, string>? GetControlValue(Control? control)
    {
        return ScriptGenControlsStaticMethods.GetControlValue(control);
    }

    #endregion

    #region Events

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void BtnGenScript_Click(object sender, RoutedEventArgs e)
    {
        /*try
        {
            string finalScript =
                ScriptGenerationStaticMethods.ReplaceParameters(ModelSqlTemplateText ?? string.Empty, UserParamValues);
            string fileToSaveName = $"{SelectedModel.Name}_{DateTime.Now.ToFileTime()}.sql";
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                DefaultExt = ".sql",
                Filter = "sql files (*.sql)|*.sql",
                AddExtension = true,
                FileName = fileToSaveName
            };
            bool? dialogRes = saveFileDialog.ShowDialog();
            if (dialogRes.HasValue && dialogRes.Value)
            {
                fileToSaveName = saveFileDialog.FileName;
                File.WriteAllText(fileToSaveName, finalScript);
            }
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message);
        }*/
    }

    #endregion
}