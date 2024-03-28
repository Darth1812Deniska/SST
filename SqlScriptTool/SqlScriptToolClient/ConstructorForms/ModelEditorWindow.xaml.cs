using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SqlScriptToolClient.Constants;
using SqlScriptToolClient.ConstructorStorage;
using SSTCommonClasses.Constants;
using SSTCommonClasses.Constructor.Models;
using SSTCommonClasses.Constructor.Tags;

namespace SqlScriptToolClient
{
    /// <summary>
    /// Логика взаимодействия для ModelEditorWindow.xaml
    /// </summary>
    public partial class ModelEditorWindow : Window
    {
        #region fields

        private bool _isSaveAndCloseClicked = false;

        #endregion

        #region properties

        private Model SelectedModel { get; set; }
        private ModelEditorFormState FormState { get; }
        private string ModelName => GetModelName();
        private string ModelDisplayLabel => GetModelDisplayLabel();
        private string ModelDescription => GetModelDescription();
        private Guid ModelUuid { get; set; }
        private DbType ModelDbType => GetModelDbType();
        private ModelItem ModelRootItem => GetModelRootItem();
        public Model CurrentModel => GetCurrentModel();
        private TreeViewItem VisualRootModelItem => GetVisualRootModelItem();
        private int AllNodesCount => GetAllNodesCount();

        #endregion

        #region Getter-setter

        private string GetModelName()
        {
            return TbName.Text.Trim();
        }

        private int GetAllNodesCount()
        {
            var curItems = TvModelTree.Items;
            return curItems.Count + GetNodesCount(curItems);
        }

        private Model GetCurrentModel()
        {
            Model result = new Model(ModelUuid, ModelName, ModelDescription, ModelDbType, ModelDisplayLabel,
                String.Empty, AccessType.Private, 0, ModelRootItem);

            return result;
        }

        private DbType GetModelDbType()
        {
            if (RbPostgreSql.IsChecked != null && RbPostgreSql.IsChecked.Value)
                return DbType.PostgreSql;
            if (RbMsSql.IsChecked != null && RbMsSql.IsChecked.Value)
                return DbType.MsSql;
            return DbType.None;
        }

        private string GetModelDisplayLabel()
        {
            return TbDisplayLabel.Text.Trim();
        }

        private string GetModelDescription()
        {
            return TbDescription.Text.Trim();
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
        /// Открытие формы на редактирование существующией модели
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
                var childModelItem = nodeModelItem.AddChildModelItem(childModelItemName, ModelDbType);

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
            if (string.IsNullOrEmpty(TbName.Text.Trim()))
            {
                MessageBox.Show("Не заполнено поле \"Название\"");
                return false;
            }

            if (string.IsNullOrEmpty(TbDisplayLabel.Text.Trim()))
            {
                MessageBox.Show("Не заполнено поле \"Отображаемое наименование\"");
                return false;
            }

            if (string.IsNullOrEmpty(TbDescription.Text.Trim()))
            {
                MessageBox.Show("Не заполнено поле \"Описание модели\"");
                return false;
            }

            if (!(RbMsSql.IsChecked.GetValueOrDefault() || RbPostgreSql.IsChecked.GetValueOrDefault()))
            {
                MessageBox.Show("Не выбран Тип СУБД");
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
            var childList = modelItem.ChildModelItems.Select(dict => dict.Value).ToList();
            foreach (var item in childList)
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
            foreach (TreeViewItem item in items)
            {
                var childItems = item.Items;
                resultCount += childItems.Count;
                resultCount += GetNodesCount(childItems);
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
            var rootModelItem = model.RootModelItem;

            DrawModelNode(TvModelTree.Items, rootModelItem);
        }

        private void Save()
        {
            if (CheckFields())
            {
                StaticConstructorStorage.SaveModel(CurrentModel);
            }
            else
            {
                MessageBox.Show("Поля не заполнены");
            }
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
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void BtnSaveAndClose_Click(object sender, RoutedEventArgs e)
        {
            Save();
            _isSaveAndCloseClicked = true;
            Close();
        }

        /// <summary>
        /// Обработка кнопки отменить
        /// </summary>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnAddChildNode_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem? selItem = TvModelTree.SelectedItem as TreeViewItem;
            if (selItem != null) // Если узел выбран
            {
                AddNodeToTreeViewItem(selItem);
            }
            else // Если узел не выбран - добавляем дочерний к корню
            {
                var rootTreeViewItem = TvModelTree.Items[0] as TreeViewItem;
                if (rootTreeViewItem != null)
                {
                    AddNodeToTreeViewItem(rootTreeViewItem);
                }
            }
        }

        private void BtnAddNode_Click(object sender, RoutedEventArgs e)
        {
            var selItem = TvModelTree.SelectedItem;
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

        private void BtnEditNode_Click(object sender, RoutedEventArgs e)
        {
            var selItem = TvModelTree.SelectedItem;
            var selTreeViewItem = selItem as TreeViewItem;

            if (selTreeViewItem?.Tag is  ModelItem selModelItem)
            {
                // Открываем окно для выбранного шаблона
                SqlTemplateEditorWindow sqlTemplateEditorWindow = new SqlTemplateEditorWindow(selModelItem.SqlTemplate);
                sqlTemplateEditorWindow.ShowDialog();
                // подтягиваем изменения
                selModelItem.SqlTemplate = sqlTemplateEditorWindow.CurrentSqlTemplate;

                // Обновляем дочерние узлы
                // Как?
                // Удаляем дочерние узлы для выбранного узла
                selTreeViewItem.Items.Clear(); // Удалятся ли все?

                // Создаем новые
                // Получаем из шаблона тэги с типом 
                var childTemplates = selModelItem.SqlTemplate.Tags
                    .Where(tag => tag.TagType == TagType.ModelItemParent).ToList();

                // проходимся по ним
                foreach (var template in childTemplates)
                {
                    // Обновить ChildItem
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

                }
            }
        }

        private void BtnDeleteNode_Click(object sender, RoutedEventArgs e)
        {
            var selItem = TvModelTree.SelectedItem;
            if (selItem is not TreeViewItem selTreeViewItem)
            {
                return;
            }

            var parentItem = selTreeViewItem.Parent;

            var parentTreeViewItem = parentItem as TreeViewItem;

            // Проверка на дочерние узлы
            if (selTreeViewItem.Items.Count > 0)
            {
                var mbResult = MessageBox.Show(
                    "У узла есть дочерние элементы. Прикрепить элементы к родительскому удаляемого?",
                    "Удаление узла",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                if (mbResult == MessageBoxResult.Cancel)
                {
                    return;
                }

                if (mbResult == MessageBoxResult.Yes)
                {
                    foreach (var item in selTreeViewItem.Items)
                    {
                        var treeViewItem = item as TreeViewItem;
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
                var mbResult = MessageBox.Show(
                    "Удалить выбранный узел?",
                    "Удаление узла",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (mbResult == MessageBoxResult.No)
                {
                    return;
                }

                if (mbResult == MessageBoxResult.Yes)
                {
                    parentTreeViewItem?.Items.Remove(selTreeViewItem);
                }
            }
        }

        private void TvModelTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SetTreeButtonsEnabled();
        }

        private void TbName_TextChanged(object sender, TextChangedEventArgs e)
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

        private void TbDisplayLabel_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (VisualRootModelItem.Tag is ModelItem rootModelItem)
            {
                rootModelItem.SqlTemplate.SetDisplayLabel(ModelDisplayLabel);
            }
        }

        private void TbDescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (VisualRootModelItem.Tag is ModelItem rootModelItem)
            {
                rootModelItem.SqlTemplate.SetDescription(ModelDescription);
            }
        }

        private void RbPostgreSql_Checked(object sender, RoutedEventArgs e)
        {
            SetRootSqlTemplateDbType();
        }

        private void RbMsSql_Checked(object sender, RoutedEventArgs e)
        {
            SetRootSqlTemplateDbType();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_isSaveAndCloseClicked)
            {
                DialogResult = true;
                return;
            }

            var result = MessageBox.Show("Закрыть окно? Несохраненные изменения будут отменены!", "Закрыть окно?",
                MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
            DialogResult = true;
        }

        #endregion
    }
}