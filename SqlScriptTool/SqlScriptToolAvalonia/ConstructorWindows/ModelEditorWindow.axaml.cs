using System;
using System.Collections.Generic;
using System.Linq;
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
        // Для окна создания модели корневой узел инициализируется в InitializeModelTree();
        // если он ещё не готов, используем корневой узел выбранной модели — его ленивый
        // getter SqlTemplate создаст шаблон при первом обращении к атрибутам.
        if (TvModelTree.Items.Count > 0 && TvModelTree.Items[0] is TreeViewItem rootTreeViewItem
            && rootTreeViewItem.Tag is ModelItem rootItem)
        {
            return rootItem;
        }

        return SelectedModel.RootModelItem;
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
            // Имя нового узла совпадает с именем sql-шаблона, который будет к нему
            // прикреплён: <ИмяМодели_ModelItem_N>. Для корневой модели — "<Модель>".
            string childModelItemName = GetNewNodeName(nodeModelItem);
            ModelItem childModelItem = nodeModelItem.AddChildModelItem(childModelItemName, ModelDbType);

            TreeViewItem newChildTreeViewItem = new TreeViewItem()
            {
                Header = childModelItemName,
                IsExpanded = true,
                Tag = childModelItem
            };
            node.Items.Add(newChildTreeViewItem);
            // Новый дочерний узел должен сразу оказаться в поле зрения
            node.IsExpanded = true;
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
            Header = modelItem.EffectiveSqlTemplate.Name,
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

    private string GetNewNodeName(ModelItem parentNode)
    {
        // Имя нового узла совпадает с именем его sql-шаблона.
        // Для корневой модели: "<ИмяМодели_ModelItem_N>"; если имя модели не введено — "<ModelItem_N>".
        // Для дочерних узлов, созданных через кнопку «Добавить дочерний узел»:
        // родительский узел называется так же, как его шаблон, поэтому имя дочернего узла
        // формируется по шаблону "<ИмяРодителя_ModelItem_N>" (унаследованное имя).
        string parentName = parentNode.EffectiveSqlTemplate.Name;
        if (string.IsNullOrEmpty(parentName))
        {
            return $"<ModelItem_{AllNodesCount}>";
        }

        if (parentName.StartsWith('<') && parentName.EndsWith('>'))
        {
            // Имя вида "<...>" — добавляем счётчик внутрь скобок: "<..._ModelItem_N>"
            string inner = parentName[1..^1];
            return $"<{inner}_ModelItem_{AllNodesCount}>";
        }

        return $"<{parentName}_ModelItem_{AllNodesCount}>";
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
        // Создать коренной узел (используется тот же ModelItem, что и у выбранной модели,
        // чтобы лениво созданный sql-шаблон корневого узла не терялся при сохранении)
        ModelItem rootModelItem = SelectedModel.RootModelItem;
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
            rootModelItem.EffectiveSqlTemplate.SetDbType(ModelDbType);
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

    /// <summary>
    /// Построение имени дочернего узла на основе имени родительского узла/шаблона.
    /// Имя дочернего узла наследуется от родителя: "<ИмяРодителя_ModelItem_N>".
    /// </summary>
    private string BuildChildName(string parentName, int childNumber)
    {
        if (string.IsNullOrEmpty(parentName))
        {
            return $"<ModelItem_{childNumber}>";
        }

        if (parentName.StartsWith('<') && parentName.EndsWith('>'))
        {
            return $"<{parentName[1..^1]}_ModelItem_{childNumber}>";
        }

        return $"<{parentName}_ModelItem_{childNumber}>";
    }

    /// <summary>
    /// Рекурсивное обновление заголовков узлов дерева и ключей ChildModelItems
    /// при изменении имени шаблона узла. Дочерний узел всегда называется так же,
    /// как sql-шаблон, который его содержит; имя потомков наследуется от имени родителя.
    /// Если новое имя совпадает с текущим — ничего не выполняется.
    /// </summary>
    /// <param name="treeViewItem">Визуальный узел дерева</param>
    /// <param name="modelItem">Узел модели, привязанный к визуальному узлу</param>
    /// <param name="newTemplateName">Новое имя sql-шаблона данного узла</param>
    private void UpdateNodeNamesRecursively(TreeViewItem treeViewItem, ModelItem modelItem, string newTemplateName)
    {
        if (string.IsNullOrWhiteSpace(newTemplateName))
        {
            return;
        }

        // Текущее имя узла (заголовок дерева) не изменилось — ничего не делаем
        string currentHeader = treeViewItem.Header?.ToString() ?? string.Empty;
        if (currentHeader == newTemplateName)
        {
            return;
        }

        // Обновляем визуальный заголовок узла и имя его sql-шаблона
        treeViewItem.Header = newTemplateName;
        modelItem.EffectiveSqlTemplate.SetName(newTemplateName);

        // Пересобираем словарь дочерних узлов под новыми ключами и рекурсивно
        // обновляем имена потомков (наследуются от имени родителя)
        List<KeyValuePair<string, ModelItem>> oldChildren = modelItem.ChildModelItems.ToList();
        modelItem.ChildModelItems.Clear();

        int childIndex = 0;
        foreach (KeyValuePair<string, ModelItem> child in oldChildren)
        {
            childIndex++;
            string childNewName = BuildChildName(newTemplateName, childIndex);

            ModelItem childModelItem = child.Value;
            childModelItem.EffectiveSqlTemplate.SetName(childNewName);
            modelItem.ChildModelItems[childNewName] = childModelItem;

            // Ищем соответствующий визуальный дочерний узел: сначала по объекту Tag,
            // затем по старому имени (если дерево было построено из сохранённой модели)
            TreeViewItem? childTreeViewItem = treeViewItem.Items
                .OfType<TreeViewItem>()
                .FirstOrDefault(item => ReferenceEquals(item.Tag, childModelItem))
                            ?? treeViewItem.Items
                                .OfType<TreeViewItem>()
                                .FirstOrDefault(item => item.Header?.ToString() == child.Key);

            if (childTreeViewItem != null)
            {
                UpdateNodeNamesRecursively(childTreeViewItem, childModelItem, childNewName);
            }
        }
    }

    /// <summary>
    /// Синхронизация структуры дерева под выбранным узлом с тэгами типа
    /// <see cref="TagType.ModelItemParent"/> из его sql-шаблона. Вызывается после
    /// закрытия SqlTemplateEditorWindow. Дочерние узлы, уже созданные пользователем
    /// ранее, сохраняются (не теряются), новые тэги получают унаследованные имена.
    /// </summary>
    private void SyncChildrenWithTemplateTags(TreeViewItem selTreeViewItem, ModelItem selModelItem)
    {
        List<Tag> modelItemTags = selModelItem.SqlTemplate?.Tags
            .Where(tag => tag.TagType == TagType.ModelItemParent).ToList()
            ?? new List<Tag>();

        // Существующие дочерние визуальные узлы и их модельные узлы
        List<TreeViewItem> existingChildViewItems = selTreeViewItem.Items.OfType<TreeViewItem>().ToList();
        List<ModelItem> existingChildModelItems = existingChildViewItems
            .Where(item => item.Tag is ModelItem)
            .Select(item => (ModelItem)item.Tag!)
            .ToList();

        // Тэги, которым уже соответствует существующий дочерний узел (по шаблону или имени)
        List<Tag> matchedTags = new List<Tag>();
        List<(TreeViewItem ViewItem, ModelItem ModelItem)> retainedChildren =
            new List<(TreeViewItem, ModelItem)>();

        foreach (ModelItem childModelItem in existingChildModelItems)
        {
            Tag? matchingTag = modelItemTags.FirstOrDefault(tag =>
                (tag.SqlTemplate != null && ReferenceEquals(tag.SqlTemplate, childModelItem.SqlTemplate))
                || tag.Name == childModelItem.EffectiveSqlTemplate.Name);

            if (matchingTag == null)
            {
                continue;
            }

            matchedTags.Add(matchingTag);
            TreeViewItem? viewItem = existingChildViewItems
                .FirstOrDefault(item => ReferenceEquals(item.Tag, childModelItem));
            if (viewItem != null)
            {
                retainedChildren.Add((viewItem, childModelItem));
            }
        }

        // Новые тэги ModelItemParent, добавленные при редактировании шаблона
        List<Tag> newTags = modelItemTags.Where(tag => !matchedTags.Contains(tag)).ToList();

        // Если структура не изменилась — только обновляем заголовки (имя шаблона могло измениться)
        if (newTags.Count == 0 && retainedChildren.Count == existingChildModelItems.Count)
        {
            for (int i = 0; i < retainedChildren.Count; i++)
            {
                (TreeViewItem viewItem, ModelItem childModelItem) = retainedChildren[i];
                string childName = childModelItem.EffectiveSqlTemplate.Name;
                if (!string.IsNullOrEmpty(childName))
                {
                    viewItem.Header = childName;
                }
            }

            return;
        }

        // Полная пересборка дочерних узлов выбранного узла
        selTreeViewItem.Items.Clear();
        selModelItem.ChildModelItems.Clear();

        // Сохраняем уже существующие дочерние узлы (с их потомками)
        foreach ((TreeViewItem viewItem, ModelItem childModelItem) in retainedChildren)
        {
            string childName = childModelItem.EffectiveSqlTemplate.Name;
            if (string.IsNullOrEmpty(childName))
            {
                childName = BuildChildName(selModelItem.EffectiveSqlTemplate.Name,
                    selModelItem.ChildModelItems.Count + 1);
                childModelItem.EffectiveSqlTemplate.SetName(childName);
            }

            selModelItem.ChildModelItems[childName] = childModelItem;
            selTreeViewItem.Items.Add(viewItem);
        }

        // Добавляем дочерние узлы для новых тэгов ModelItemParent
        foreach (Tag template in newTags)
        {
            // Имя нового дочернего узла наследуется от имени родительского шаблона
            string childName = string.IsNullOrEmpty(template.Name)
                ? BuildChildName(selModelItem.EffectiveSqlTemplate.Name,
                    selModelItem.ChildModelItems.Count + 1)
                : template.Name;

            ModelItem modelItem;
            if (template.SqlTemplate != null)
            {
                // Тэг уже связан с sql-шаблоном (например, выбран через редактор тэгов)
                modelItem = new ModelItem
                {
                    SqlTemplate = template.SqlTemplate
                };
                // Узел называется так же, как шаблон, его содержащий
                if (string.IsNullOrEmpty(modelItem.SqlTemplate!.Name))
                {
                    modelItem.SqlTemplate.SetName(childName);
                }
                else
                {
                    childName = modelItem.SqlTemplate.Name;
                }

                if (!selModelItem.ChildModelItems.ContainsKey(childName))
                {
                    selModelItem.ChildModelItems[childName] = modelItem;
                }
            }
            else
            {
                // Создаём новый дочерний узел через модельный метод — он сам создаст
                // sql-шаблон, добавит тэг в текст шаблона и зарегистрирует в ChildModelItems
                modelItem = selModelItem.AddChildModelItem(childName, ModelDbType);
            }

            TreeViewItem treeViewItem = new TreeViewItem()
            {
                Header = modelItem.EffectiveSqlTemplate.Name,
                IsExpanded = true,
                Tag = modelItem
            };

            selTreeViewItem.Items.Add(treeViewItem);
        }

        selTreeViewItem.IsExpanded = true;
    }

    private async void BtnEditNode_Click(object? sender, RoutedEventArgs e)
    {
        object? selItem = TvModelTree.SelectedItem;
        TreeViewItem? selTreeViewItem = selItem as TreeViewItem;

        if (selTreeViewItem?.Tag is ModelItem selModelItem)
        {
            // Открываем окно для выбранного шаблона
            SqlTemplateEditorWindow sqlTemplateEditorWindow =
                new SqlTemplateEditorWindow(selModelItem.EffectiveSqlTemplate);
            await sqlTemplateEditorWindow.ShowDialog(this);
            // подтягиваем изменения
            selModelItem.SqlTemplate = sqlTemplateEditorWindow.CurrentSqlTemplate;

            string updatedTemplateName = sqlTemplateEditorWindow.CurrentSqlTemplate.Name;

            // Обновляем заголовок выбранного узла: узел называется так же,
            // как sql-шаблон, который он содержит
            bool rootItemSelected = ReferenceEquals(selTreeViewItem, VisualRootModelItem);
            if (!string.IsNullOrWhiteSpace(updatedTemplateName) && rootItemSelected)
            {
                // Изменён корневой узел: обновляем имя модели и рекурсивно
                // переименовываем все дочерние узлы (без потери потомков)
                TbName.Text = updatedTemplateName.Trim('<', '>');
                UpdateNodeNamesRecursively(selTreeViewItem, selModelItem, updatedTemplateName);
            }
            else if (!string.IsNullOrWhiteSpace(updatedTemplateName))
            {
                selTreeViewItem.Header = updatedTemplateName;
            }

            // Синхронизируем дочерние узлы с тэгами ModelItemParent из обновлённого шаблона
            SyncChildrenWithTemplateTags(selTreeViewItem, selModelItem);
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
        // Переименовываем название тэга - шаблона.
        // Используем EffectiveSqlTemplate: при создании новой модели корневой узел
        // ещё не имеет sql-шаблона — ленивый getter создаёт его при первом обращении,
        // поэтому NullReferenceException больше не возникает.
        if (VisualRootModelItem.Tag is ModelItem rootModelItem)
        {
            rootModelItem.EffectiveSqlTemplate.SetName(nodeName);

            // Рекурсивно обновляем имена дочерних узлов, унаследованные от имени
            // корневого шаблона (узел называется так же, как шаблон его содержащий).
            // Если имя не изменилось — метод ничего не делает.
            UpdateNodeNamesRecursively(VisualRootModelItem, rootModelItem, nodeName);
        }
    }

    private void TbDisplayLabel_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (VisualRootModelItem.Tag is ModelItem rootModelItem)
        {
            rootModelItem.EffectiveSqlTemplate.SetDisplayLabel(ModelDisplayLabel);
        }
    }

    private void TbDescription_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (VisualRootModelItem.Tag is ModelItem rootModelItem)
        {
            rootModelItem.EffectiveSqlTemplate.SetDescription(ModelDescription);
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
