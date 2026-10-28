using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia.Enums;
using SqlScriptToolAvalonia.Constants;
using SqlScriptToolAvalonia.ConstructorForms;
using SqlScriptToolAvalonia.ConstructorStorage;
using SqlScriptToolAvalonia.Helpers;
using SqlScriptToolClient.ConstructorStorage;
using SSTCommonClasses.Constants;
using SSTCommonClasses.Constructor.Models;
using SSTCommonClasses.Constructor.Tags;

namespace SqlScriptToolAvalonia.ConstructorForms;

/// <summary>
/// Логика взаимодействия для ModelEditorWindow.axaml
/// </summary>
public partial class ModelEditorWindow : Window
{
    #region fields

    private bool _isSaveAndCloseClicked = false;

    #endregion

    #region properties

    /// <summary>
    /// Выбранная модель для редактирования
    /// </summary>
    private Model SelectedModel { get; }
    /// <summary>
    /// Статус открытой формы
    /// </summary>
    private ModelEditorFormState FormState { get; }
    /// <summary>
    /// Название модели
    /// </summary>
    private string ModelName => GetModelName();
    /// <summary>
    /// Отображаемое название модели
    /// </summary>
    private string ModelDisplayLabel => GetModelDisplayLabel();
    /// <summary>
    /// краткое описание модели
    /// </summary>
    private string ModelDescription => GetModelDescription();
    /// <summary>
    /// Уникальный идентификатор модели
    /// </summary>
    private Guid ModelUuid { get; set; }
    /// <summary>
    /// Выбранный тип БД
    /// </summary>
    private DbType ModelDbType => GetModelDbType();
    /// <summary>
    /// Коренной узел модели
    /// </summary>
    private ModelItem ModelRootItem => GetModelRootItem();
    /// <summary>
    /// Текущая редактируемая модель
    /// </summary>
    public Model CurrentModel => GetCurrentModel();
    /// <summary>
    /// Визуальный узел на дереве структуры модели
    /// </summary>
    private TreeViewItem VisualRootModelItem => GetVisualRootModelItem();
    // Количество всех узлов в модели
    private int AllNodesCount => GetAllNodesCount();

    #endregion

    #region Getter-setter

    private string GetModelName()
    {
        return TbName.Text?.Trim() ?? string.Empty;
    }

    private int GetAllNodesCount()
    {
        ItemCollection curItems = TvModelTree.Items;
        return curItems.Count + GetNodesCount(curItems);
    }

    private Model GetCurrentModel()
    {
        Model result = new Model(ModelUuid, ModelName, ModelDescription, ModelDbType, ModelDisplayLabel,
            string.Empty, AccessType.Private, 0, ModelRootItem);

        return result;
    }

    private DbType GetModelDbType()
    {
        if (RbPostgreSql.IsChecked == true)
            return DbType.PostgreSql;
        if (RbMsSql.IsChecked == true)
            return DbType.MsSql;
        return DbType.None;
    }

    private string GetModelDisplayLabel()
    {
        return TbDisplayLabel.Text?.Trim() ?? string.Empty;
    }

    private string GetModelDescription()
    {
        return TbDescription.Text?.Trim() ?? string.Empty;
    }

    private ModelItem GetModelRootItem()
    {
        ModelItem result = VisualRootModelItem.Tag as ModelItem ?? new ModelItem();
        return result;
    }

    private TreeViewItem GetVisualRootModelItem()
    {
        if (TvModelTree.Items.Count == 0)
        {
            return new TreeViewItem();
        }

        return TvModelTree.Items[0] as TreeViewItem ?? new TreeViewItem();
    }

    #endregion


    #region Constructors

    /// <summary>
    /// Открытие формы на создание новой модели
    /// </summary>
    public ModelEditorWindow()
    {
        InitializeComponent();
        FormState = ModelEditorFormState.Creation;
        SelectedModel = new Model();
        SelectedModel.InitRootModelItem();
        InitializeProperties();
    }

    /// <summary>
    /// Открытие формы на редактирование существующей модели
    /// </summary>
    /// <param name="model"></param>
    public ModelEditorWindow(Model model)
    {
        InitializeComponent();
        FormState = ModelEditorFormState.Editing;
        SelectedModel = model;
        InitializeProperties();
    }

    #endregion

    #region private methods

    private void AddNodeToTreeViewItem(TreeViewItem node)
    {
        if (node.Tag is ModelItem nodeModelItem)
        {
            string childModelItemName = GetNewNodeName();
            ModelItem childModelItem = nodeModelItem.AddChildModelItem(childModelItemName, ModelDbType);

            TreeViewItem newChildTreeViewItem = new TreeViewItem()
            {
                Header = childModelItemName,
                IsExpanded = true,
                Tag = childModelItem
            };
            node.Items.Add(newChildTreeViewItem);
        }
    }

    /// <summary>
    /// Проверка полей перед сохранением
    /// </summary>
    /// <returns></returns>
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

        return true;
    }

    private void DrawModelNode(ItemCollection items, ModelItem? modelItem)
    {
        if (modelItem == null)
        {
            return;
        }

        TreeViewItem treeViewItem = new TreeViewItem()
        {
            Header = modelItem.SqlTemplate.Name,
            IsExpanded = true
        };
        treeViewItem.Tag = modelItem;
        items.Add(treeViewItem);
        List<ModelItem> childList = modelItem.ChildModelItems.Select(dict => dict.Value).ToList();
        foreach (ModelItem item in childList)
        {
            DrawModelNode(treeViewItem.Items, item);
        }
    }

    private string GetNewNodeName()
    {
        return $"<ModelItem_{AllNodesCount}>";
    }

    private int GetNodesCount(ItemCollection items)
    {
        int resultCount = 0;
        foreach (object? itemObject in items)
        {
            if (itemObject is TreeViewItem item)
            {
                ItemCollection childItems = item.Items;
                resultCount += childItems.Count;
                resultCount += GetNodesCount(childItems);
            }
        }

        return resultCount;
    }

    /// <summary>
    /// Инициализация свойств и заполнение полей
    /// </summary>
    private void InitializeProperties()
    {
        switch (FormState)
        {
            case ModelEditorFormState.Creation:
                ModelUuid = Guid.NewGuid();
                TbUuid.Text = ModelUuid.ToString();
                InitializeModelTree();
                break;
            case ModelEditorFormState.Editing:
                ModelUuid = SelectedModel.Uuid;
                TbUuid.Text = ModelUuid.ToString();
                TbName.Text = SelectedModel.Name;
                TbDisplayLabel.Text = SelectedModel.DisplayLabel;
                TbDescription.Text = SelectedModel.Description;
                SetModelDbType();
                InitializeModelTree(SelectedModel);
                break;
        }

        SetTreeButtonsEnabled();
    }

    /// <summary>
    /// Инициализация дерева новой модели
    /// </summary>
    private void InitializeModelTree()
    {
        // Создать коренной узел
        ModelItem rootModelItem = new ModelItem();
        // Добавить визуальный коренной узел
        TreeViewItem rootNodeItem = new TreeViewItem()
        {
            Header = "<ModelItem>",
            IsExpanded = true
        };
        rootNodeItem.Tag = rootModelItem;
        TvModelTree.Items.Add(rootNodeItem);
    }

    /// <summary>
    /// Отрисовка уже существующего дерева модели
    /// </summary>
    /// <param name="model"></param>
    private void InitializeModelTree(Model model)
    {
        ModelItem rootModelItem = model.RootModelItem;

        DrawModelNode(TvModelTree.Items, rootModelItem);
    }

    private bool Save()
    {
        if (CheckFields())
        {
            try
            {
                StaticConstructorStorage.SaveModel(CurrentModel);
            }
            catch (Exception e)
            {
                AvaloniaMessageBox.ShowError(e.Message, "Ошибка при сохранении");
                return false;
            }

            return true;
        }

        return false;
    }

    private void SetModelDbType()
    {
        switch (SelectedModel.DbType)
        {
            case DbType.PostgreSql:
                RbPostgreSql.IsChecked = true;
                break;
            case DbType.MsSql:
                RbPostgreSql.IsChecked = true;
                break;
        }
    }

    private void SetTreeButtonsEnabled()
    {
        BtnAddChildNode.IsEnabled = false;
        BtnAddNode.IsEnabled = false;
        BtnEditNode.IsEnabled = false;
        BtnDeleteNode.IsEnabled = false;
        if (TvModelTree.SelectedItem == null)
        {
            BtnAddChildNode.IsEnabled = true;
        }
        else
        {
            // временная затычка, чтобы не обрабатывать коренной узел
            BtnAddChildNode.IsEnabled = true;
            BtnAddNode.IsEnabled = true;
            BtnEditNode.IsEnabled = true;
            BtnDeleteNode.IsEnabled = true;
        }
    }

    private void SetRootSqlTemplateDbType()
    {
        if (VisualRootModelItem.Tag is ModelItem rootModelItem)
        {
            rootModelItem.SqlTemplate.SetDbType(ModelDbType);
        }
    }

    #endregion

    #region events

    /// <summary>
    /// Обработка кнопки "Сохранить"
    /// </summary>
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

    /// <summary>
    /// Обработка кнопки отменить
    /// </summary>
    private void BtnCancel_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void BtnAddChildNode_Click(object? sender, RoutedEventArgs e)
    {
        TreeViewItem? selItem = TvModelTree.SelectedItem as TreeViewItem;
        if (selItem != null) // Если узел выбран
        {
            AddNodeToTreeViewItem(selItem);
        }
        else // Если узел не выбран - добавляем дочерний к корню
        {
            TreeViewItem? rootTreeViewItem = TvModelTree.Items[0] as TreeViewItem;
            if (rootTreeViewItem != null)
            {
                AddNodeToTreeViewItem(rootTreeViewItem);
            }
        }
    }

    private void BtnAddNode_Click(object? sender, RoutedEventArgs e)
    {
        object? selItem = TvModelTree.SelectedItem;
        if (selItem != null)
        {
            if (selItem is TreeViewItem selTreeViewItem)
            {
                if (selTreeViewItem.Parent is TreeViewItem parentTreeVeiwItem)
                {
                    AddNodeToTreeViewItem(parentTreeVeiwItem);
                }
            }
        }
    }

    private async void BtnEditNode_Click(object? sender, RoutedEventArgs e)
    {
        object? selItem = TvModelTree.SelectedItem;
        TreeViewItem? selTreeViewItem = selItem as TreeViewItem;

        if (selTreeViewItem?.Tag is ModelItem selModelItem)
        {
            // Открываем окно для выбранного шаблона
            SqlTemplateEditorWindow sqlTemplateEditorWindow = new SqlTemplateEditorWindow(selModelItem.SqlTemplate);
            await sqlTemplateEditorWindow.ShowDialog(this);
            // подтягиваем изменения
            selModelItem.SqlTemplate = sqlTemplateEditorWindow.CurrentSqlTemplate;

            // Обновляем дочерние узлы
            // Удаляем дочерние узлы для выбранного узла
            selTreeViewItem.Items.Clear();
            // Чистим ChildModelItem
            selModelItem.ChildModelItems.Clear();

            // Создаем новые
            // Получаем из шаблона тэги с типом
            List<Tag> childTemplates = selModelItem.SqlTemplate.Tags
                .Where(tag => tag.TagType == TagType.ModelItemParent).ToList();

            // проходимся по ним
            foreach (Tag template in childTemplates)
            {
                // Обновить ChildItem
                if (template.SqlTemplate == null)
                {
                    continue;
                }

                ModelItem modelItem = new ModelItem()
                {
                    SqlTemplate = template.SqlTemplate
                };
                // Для каждого создаем узлы
                TreeViewItem treeViewItem = new TreeViewItem()
                {
                    Header = template.Name,
                    Tag = modelItem
                };
                selTreeViewItem.Items.Add(treeViewItem);
                // Добавляем в ChildModelItem
                selModelItem.ChildModelItems.Add(template.Name, modelItem);
            }
        }
    }

    private async void BtnDeleteNode_Click(object? sender, RoutedEventArgs e)
    {
        object? selItem = TvModelTree.SelectedItem;
        if (selItem is not TreeViewItem selTreeViewItem)
        {
            return;
        }

        object? parentItem = selTreeViewItem.Parent;

        TreeViewItem? parentTreeViewItem = parentItem as TreeViewItem;

        // Проверка на дочерние узлы
        if (selTreeViewItem.Items.Count > 0)
        {
            ButtonResult mbResult = await AvaloniaMessageBox.ShowQuestionAsync(this,
                "У узла есть дочерние элементы. Прикрепить элементы к родительскому удаляемого?",
                "Удаление узла", ButtonEnum.YesNoCancel);
            if (mbResult == ButtonResult.Cancel)
            {
                return;
            }

            if (mbResult == ButtonResult.Yes)
            {
                foreach (object? item in selTreeViewItem.Items.ToList())
                {
                    TreeViewItem? treeViewItem = item as TreeViewItem;
                    if (treeViewItem == null)
                    {
                        continue;
                    }

                    TreeViewItem newItem = new TreeViewItem()
                    {
                        Header = treeViewItem.Header,
                        Tag = treeViewItem.Tag
                    };
                    parentTreeViewItem?.Items.Add(newItem);
                }
            }

            parentTreeViewItem?.Items.Remove(selTreeViewItem);
        }
        else
        {
            ButtonResult mbResult = await AvaloniaMessageBox.ShowQuestionAsync(this,
                "Удалить выбранный узел?",
                "Удаление узла", ButtonEnum.YesNo);
            if (mbResult == ButtonResult.No)
            {
                return;
            }

            if (mbResult == ButtonResult.Yes)
            {
                parentTreeViewItem?.Items.Remove(selTreeViewItem);
            }
        }
    }

    private void TvModelTree_SelectedItemChanged(object? sender, SelectionChangedEventArgs e)
    {
        SetTreeButtonsEnabled();
    }

    private void TbName_TextChanged(object? sender, TextChangedEventArgs e)
    {
        // Наименование узла
        string nodeName = string.IsNullOrEmpty(ModelName) ? "<ModelItem>" : $"<{ModelName}>";
        // Без проверки на ввод текста
        // Находим коренной узел
        VisualRootModelItem.Header = nodeName;
        // Переименовываем название тэга - шаблона
        if (VisualRootModelItem.Tag is ModelItem rootModelItem)
        {
            rootModelItem.SqlTemplate.SetName(nodeName);
        }
    }

    private void TbDisplayLabel_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (VisualRootModelItem.Tag is ModelItem rootModelItem)
        {
            rootModelItem.SqlTemplate.SetDisplayLabel(ModelDisplayLabel);
        }
    }

    private void TbDescription_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (VisualRootModelItem.Tag is ModelItem rootModelItem)
        {
            rootModelItem.SqlTemplate.SetDescription(ModelDescription);
        }
    }

    private void RbPostgreSql_Checked(object? sender, RoutedEventArgs e)
    {
        SetRootSqlTemplateDbType();
    }

    private void RbMsSql_Checked(object? sender, RoutedEventArgs e)
    {
        SetRootSqlTemplateDbType();
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
