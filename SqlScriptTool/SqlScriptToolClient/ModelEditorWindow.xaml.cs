using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SqlScriptToolClient.Constants;
using SSTCommonClasses.Models;

namespace SqlScriptToolClient
{
    /// <summary>
    /// Логика взаимодействия для ModelEditorWindow.xaml
    /// </summary>
    public partial class ModelEditorWindow : Window
    {
        #region properties

        private Model SelectedModel { get; set; }
        private ModelEditorFormState FormState { get; set; }
        private string ModelName => GetModelName();
        private Guid ModelUuid { get; set; }
        private TreeViewItem VisualRootModelItem => TvModelTree.Items[0] as TreeViewItem ?? new TreeViewItem();

        #endregion

        private string GetModelName()
        {
            return TbName.Text.Trim();
        }


        /// <summary>
        /// Открытие формы на создание новой модели
        /// </summary>
        public ModelEditorWindow()
        {
            InitializeComponent();
            FormState = ModelEditorFormState.Creation;
            SelectedModel = new Model();
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
                Header = "ModelItem",
                IsExpanded = true
            };
            rootNodeItem.Tag = rootModelItem;
            TvModelTree.Items.Add(rootNodeItem);
        }

        //private 

        /// <summary>
        /// Отрисовка уже существующего дерева модели
        /// </summary>
        /// <param name="model"></param>
        private void InitializeModelTree(Model model)
        {
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

        /// <summary>
        /// Обработка кнопки "Сохранить"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (CheckFields())
            {
                MessageBox.Show("Поля заполнены");
            }
            else
            {
                MessageBox.Show("Поля не заполнены");
            }
        }

        /// <summary>
        /// Обработка кнопки отменить
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnAddChildNode_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem? selItem = TvModelTree.SelectedItem as TreeViewItem;
            TreeViewItem newChildTreeViewItem = new TreeViewItem()
            {
                Header = "ModelItem",
                IsExpanded = true
            };
            if (selItem != null) // Если узел выбран
            {
                selItem.Items.Add(newChildTreeViewItem);
            }
            else // Если узел не выбран - добавляем дочерний к корню
            {
                var rootTreeViewItem = TvModelTree.Items[0] as TreeViewItem;
                if (rootTreeViewItem != null)
                {
                    rootTreeViewItem.Items.Add(newChildTreeViewItem);
                }
            }
        }

        private void BtnAddNode_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem? selItem = TvModelTree.SelectedItem as TreeViewItem;
            if (selItem.Parent is TreeViewItem parentTreeVeiwItem)
            {
                TreeViewItem newChildTreeViewItem = new TreeViewItem()
                {
                    Header = "ModelItem",
                    IsExpanded = true
                };
                parentTreeVeiwItem.Items.Add(newChildTreeViewItem);
            }
        }


        private void BtnEditNode_Click(object sender, RoutedEventArgs e)
        {
        }

        private void BtnDeleteNode_Click(object sender, RoutedEventArgs e)
        {
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
            var rootModelItem = VisualRootModelItem.Tag as ModelItem;
            if (rootModelItem != null)
            {
                rootModelItem.SqlTemplate.SetName(nodeName);
            }
        }
    }
}