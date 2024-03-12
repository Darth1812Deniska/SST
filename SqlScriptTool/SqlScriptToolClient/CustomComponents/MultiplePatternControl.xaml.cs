using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SqlScriptToolClient.ScriptGeneration;
using SSTCommonClasses.Models;
using SSTCommonClasses.Tags;
using Xceed.Wpf.Toolkit;
using Label = System.Windows.Controls.Label;

namespace SqlScriptToolClient.CustomComponents
{
    /// <summary>
    /// Логика взаимодействия для MultiplePatternControl.xaml
    /// </summary>
    public partial class MultiplePatternControl : UserControl
    {
        #region private fields

        private Image _deleteButtonImage;

        #endregion


        #region Constructors

        public MultiplePatternControl(SqlTemplate sqlTemplate)
        {
            InitializeComponent();
            SelectedSqlTemplate = sqlTemplate;
            LblPaternName.Content = SelectedSqlTemplate.DisplayLabel;
            RowGuids = new Dictionary<Guid, List<int>>();
            _deleteButtonImage = new Image();
            Uri imageUri = new Uri("pack://application:,,,/pics/icon_remove_button.png");
            _deleteButtonImage.Source = new BitmapImage(imageUri);
            // отрисовка строки с заголовком грида
            AddDisplayLabelsRow();
            // отрисовка строки с контролами
            AddNewFieldsRow();
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// Выбранный SQL-шаблон
        /// </summary>
        private SqlTemplate SelectedSqlTemplate { get; }

        /// <summary>
        /// Список тэгов
        /// </summary>
        // Список тэгов, по которым строим заголовок и подбираем контролы
        private List<Tag> Tags => SelectedSqlTemplate.Tags;

        private string SqlText => SelectedSqlTemplate.SqlText;

        /// <summary>
        /// Список строк с индексами контролов в этих строках
        /// </summary>
        private Dictionary<Guid, List<int>> RowGuids { get; }

        // Инициализированное изображение для кнопки
        private Image DeleteButtonImage => _deleteButtonImage;

        #endregion

        #region Public properties

        /// <summary>
        /// Список выбранных параметров
        /// </summary>
        public List<Dictionary<string, string>> MultiplePatternsParamValues => GetMultiplePatternsParamValues();

        /// <summary>
        /// Собранный текст скрипта с подставленными параметрами
        /// </summary>
        public string MultiplePatternsText => GetMultiplePatternsText();

        //public bool IsCompletelyFilled => GetIsCompletelyFilled();
        //public List<string[]> BlankParameters => GetBlankParameters();

        #endregion

        #region Getters-setters

        private string GetMultiplePatternsText()
        {
            string result = string.Empty;
            foreach (var patternsParamValue in MultiplePatternsParamValues)
            {
                string patternText = ScriptGenerationStaticMethods.ReplaceParameters(SqlText, patternsParamValue);
                result = string.Concat(result, '\n', patternText);
            }

            return result;
        }

        private List<Dictionary<string, string>> GetMultiplePatternsParamValues()
        {
            var result = new List<Dictionary<string, string>>();
            // Взять только те строки, в которых есть поля для заполнения
            foreach (var rowGuid in RowGuids)
            {
                Dictionary<string, string> row = new Dictionary<string, string>();
                var controlsIndex = rowGuid.Value;
                foreach (var index in controlsIndex)
                {
                    Control? control = GrMain.Children[index] as Control;

                    var controlValue = GetControlValue(control);
                    if (controlValue.HasValue)
                    {
                        row.Add(controlValue.Value.Key, controlValue.Value.Value);
                    }
                }

                result.Add(row);
            }

            return result;
        }

        /*
        private bool GetIsCompletelyFilled()
        {
            return false;
        }

        private List<string[]> GetBlankParameters()
        {
            return new List<string[]>();
        }
        */

        #endregion


        #region Private methods

        private void AddDeleteButtonFowRow(Guid guid, int curColumn, int curRow)
        {
            Image btnImage = new Image();
            btnImage.Source = DeleteButtonImage.Source;
            Button btnDelRow = new Button()
            {
                Content = btnImage,
                Width = 20,
                Height = 20,
                // Связываем кнопку со строкой гуидом
                Tag = guid
            };
            btnDelRow.Click += BtnDelRow_Click;
            GrMain.Children.Add(btnDelRow);
            // Привязываем к последней колонке (для кнопок)
            Grid.SetColumn(btnDelRow, curColumn);
            // Привязываем к этой же строке
            Grid.SetRow(btnDelRow, curRow);
        }

        private void AddDisplayLabelsRow()
        {
            int columnCount = 0;

            foreach (var tag in Tags)
            {
                string displayLabel = tag.DisplayLabel ?? tag.Name;
                Label label = new Label
                {
                    Content = displayLabel,
                    // Привязка тэга к лейблу
                    Tag = tag
                };
                //Создаем колонки
                ColumnDefinition cd = new ColumnDefinition()
                {
                    Width = GridLength.Auto
                };
                GrMain.ColumnDefinitions.Add(cd);

                GrMain.Children.Add(label);
                Grid.SetColumn(label, columnCount);
                Grid.SetRow(label, 0);
                columnCount++;
            }

            // Добавляем колнку для кнопки удаления
            ColumnDefinition btnColumn = new ColumnDefinition()
            {
                Width = GridLength.Auto
            };
            GrMain.ColumnDefinitions.Add(btnColumn);
        }

        private void AddNewFieldsRow()
        {
            int columnCount = 0;
            Guid columnGuid = Guid.NewGuid();

            RowDefinition rd = new RowDefinition()
            {
                Height = GridLength.Auto
            };
            GrMain.RowDefinitions.Add(rd);
            int curRow = GrMain.RowDefinitions.Count - 1;
            var controlList = new List<int>();
            foreach (var tag in Tags)
            {
                var newFieldControl = FieldControlByDataType(tag);
                if (newFieldControl != null)
                {
                    newFieldControl.Margin = new Thickness(2);
                    newFieldControl.MinWidth = 100;
                    // Привязка тэга к контролу
                    KeyValuePair<Guid, Tag> controlTag = new KeyValuePair<Guid, Tag>(columnGuid, tag);
                    newFieldControl.Tag = controlTag;
                    int position = GrMain.Children.Add(newFieldControl);
                    // Собираем индексы контролов в гриде
                    controlList.Add(position);
                    Grid.SetColumn(newFieldControl, columnCount);
                    Grid.SetRow(newFieldControl, curRow);
                }

                columnCount++;
            }

            // Указываем список индексов и привязываем к гуиду
            RowGuids.Add(columnGuid, controlList);
            // добавляем кнопку на удаление
            AddDeleteButtonFowRow(columnGuid, columnCount, curRow);
        }

        private Control? FieldControlByDataType(Tag tag)
        {
            Control? result = null;
            var dataType = tag.DataType;
            switch (dataType)
            {
                case TagDataType.Char:
                    CheckBox checkBox = new CheckBox()
                    {
                        Content = tag.DisplayLabel
                    };
                    result = checkBox;
                    break;
                case TagDataType.Date:
                    DateTimePicker dateTimePicker = new DateTimePicker()
                    {
                        Format = DateTimeFormat.ShortDate,
                        TimePickerVisibility = Visibility.Collapsed
                    };
                    result = dateTimePicker;
                    break;
                case TagDataType.DateTime:
                    DateTimePicker datePicker = new DateTimePicker()
                    {
                        Format = DateTimeFormat.SortableDateTime
                    };
                    result = datePicker;
                    break;
                case TagDataType.Double:
                    DoubleUpDown doubleUpDown = new DoubleUpDown();
                    result = doubleUpDown;
                    break;
                case TagDataType.Int:
                    IntegerUpDown intUpDown = new IntegerUpDown();
                    result = intUpDown;
                    break;
                case TagDataType.LdObject:
                    ComboBox ldObjectComboBox = new ComboBox();
                    result = ldObjectComboBox;
                    break;
                case TagDataType.LdObjectType:
                    ComboBox ldObjectTypeComboBox = new ComboBox();
                    result = ldObjectTypeComboBox;
                    break;
                case TagDataType.String:
                    TextBox textBox = new TextBox();
                    result = textBox;
                    break;
            }

            return result;
        }

        private KeyValuePair<string, string>? GetControlValue(Control? control)
        {
            KeyValuePair<string, string>? result = null;
            if (control == null)
            {
                return null;
            }

            if (control.Tag is KeyValuePair<Guid, Tag> controlTag)
            {
                Tag tag = controlTag.Value;
                string tagName = tag.Name;
                TagDataType? dataType = tag.DataType;
                if (dataType == null)
                {
                    return null;
                }

                string controlValue;
                switch (dataType)
                {
                    case TagDataType.Char:
                        controlValue = (control as CheckBox).IsChecked.Value ? "+" : "-";
                        break;
                    case TagDataType.Date:
                        var datePicker = (control as DateTimePicker).Value;
                        controlValue = datePicker.HasValue
                            ? datePicker.Value.ToString("yyyyMMdd")
                            : string.Empty;
                        break;
                    case TagDataType.DateTime:
                        var dateTimePicker = (control as DateTimePicker).Value;
                        controlValue = dateTimePicker.HasValue
                            ? dateTimePicker.Value.ToString("yyyy-MM-dd HH:mm:ss")
                            : string.Empty;
                        break;
                    case TagDataType.Double:
                        var doubleControl = (control as DoubleUpDown).Value;
                        controlValue = doubleControl.HasValue
                            ? doubleControl.Value.ToString("R")
                            : "0";
                        break;
                    case TagDataType.Int:
                        var intControl = (control as IntegerUpDown).Value;
                        controlValue = intControl.HasValue ? intControl.Value.ToString() : "0";
                        break;
                    case TagDataType.LdObject:
                        controlValue = "0";
                        break;
                    case TagDataType.LdObjectType:
                        controlValue = "0";
                        break;
                    case TagDataType.String:
                        controlValue = (control as TextBox).Text;
                        break;
                    default:
                        controlValue = string.Empty;
                        break;
                }

                if (controlValue != null)
                {
                    result = new KeyValuePair<string, string>(tagName, controlValue);
                }
            }

            return result;
        }

        #endregion

        #region Events

        private void BtnAddRow_Click(object sender, RoutedEventArgs e)
        {
            AddNewFieldsRow();
        }

        private void BtnDelRow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                // Получаем гуид для поиска строки и удаления соответсвующих контрола
                Guid rowGuid = (Guid)btn.Tag;
                if (rowGuid != Guid.Empty)
                {
                    // Выбираем ту строку, в которой находится кнопка
                    List<int> controlIndexes = RowGuids[rowGuid];
                    foreach (int controlIndex in controlIndexes)
                    {
                        // Получаем контрол по его индексу
                        if (GrMain.Children[controlIndex] is Control control)
                        {
                            // Скрываем контролы
                            control.Visibility = Visibility.Collapsed;
                        }
                    }

                    // Удаляем контролы из списка для обработки
                    RowGuids.Remove(rowGuid);
                    // Скрываем кнопку
                    btn.Visibility = Visibility.Collapsed;
                }
            }
        }

        #endregion
    }
}