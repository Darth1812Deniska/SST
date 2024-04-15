using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using SqlScriptToolClient.ScriptGeneration;
using SSTCommonClasses.Constructor.Models;
using SSTCommonClasses.Constructor.Tags;
using Label = System.Windows.Controls.Label;

namespace SqlScriptToolClient.CustomComponents
{
    /// <summary>
    /// Класс компонента, позволяющего добавлять в скрипт многократно повторяющийся шаблон
    /// </summary>
    public partial class MultiplePatternControl : UserControl
    {
        #region private fields

        private Image _deleteButtonImage;

        #endregion


        #region Constructors

        /// <summary>
        /// Создание экземпляра компонента, позволяющего добавлять в скрипт многократно повторяющийся шаблон
        /// </summary>
        /// <param name="sqlTemplate">Шаблон на сонове, которого формируются поля для ввода параметров </param>
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

        /// <summary>
        /// Текст SQL-шаблон, который будет повторяться
        /// </summary>
        private string? SqlText => SelectedSqlTemplate.SqlText;

        /// <summary>
        /// Список строк с индексами контролов в этих строках
        /// </summary>
        private Dictionary<Guid, List<int>> RowGuids { get; }

        // Инициализированное изображение для кнопки
        private Image DeleteButtonImage => _deleteButtonImage;

        #endregion

        #region Public properties

        /// <summary>
        /// Список значений выбранных и заполненных параметров
        /// </summary>
        public List<Dictionary<string, string>> MultiplePatternsParamValues => GetMultiplePatternsParamValues();

        /// <summary>
        /// Собранный текст скрипта с подставленными параметрами
        /// </summary>
        public string MultiplePatternsText => GetMultiplePatternsText();

        #endregion

        #region Getters-setters

        /// <summary>
        /// Метод возвращающий собранный текст скрипта с подставленными параметрами
        /// </summary>
        /// <returns>Собранный текст скрипта с подставленными параметрами</returns>
        private string GetMultiplePatternsText()
        {
            string result = string.Empty;
            foreach (Dictionary<string, string> patternsParamValue in MultiplePatternsParamValues)
            {
                string patternText =
                    ScriptGenerationStaticMethods.ReplaceParameters(SqlText ?? string.Empty, patternsParamValue);
                result = string.Concat(result, '\n', patternText);
            }

            return result;
        }

        /// <summary>
        /// Метод возвращающий список значений выбранных и заполненных параметров
        /// </summary>
        /// <returns>Список значений выбранных и заполненных параметров</returns>
        private List<Dictionary<string, string>> GetMultiplePatternsParamValues()
        {
            List<Dictionary<string, string>> result = new List<Dictionary<string, string>>();
            // Взять только те строки, в которых есть поля для заполнения
            foreach (KeyValuePair<Guid, List<int>> rowGuid in RowGuids)
            {
                Dictionary<string, string> row = new Dictionary<string, string>();
                List<int> controlsIndex = rowGuid.Value;
                foreach (int index in controlsIndex)
                {
                    Control? control = GrMain.Children[index] as Control;

                    KeyValuePair<string, string>? controlValue = GetControlValue(control);
                    if (controlValue.HasValue)
                    {
                        row.Add(controlValue.Value.Key, controlValue.Value.Value);
                    }
                }

                result.Add(row);
            }

            return result;
        }

        #endregion


        #region Private methods

        /// <summary>
        /// Добавление строки с заголовками
        /// </summary>
        private void AddDisplayLabelsRow()
        {
            int columnCount = 0;

            foreach (Tag tag in Tags)
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

        /// <summary>
        /// Добавление новой строки с полями для ввода
        /// </summary>
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
            List<int> controlList = new List<int>();
            foreach (Tag tag in Tags)
            {
                Control? newFieldControl = FieldControlByDataType(tag);
                if (newFieldControl != null)
                {
                    newFieldControl.Margin = new Thickness(2);
                    newFieldControl.MinWidth = 100;
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

        /// <summary>
        /// Добавление кнопки удаления для строки
        /// </summary>
        /// <param name="guid">Уникальный идентификатор строки</param>
        /// <param name="curColumn">Номер колонки для добавления</param>
        /// <param name="curRow">Гомер строки для добавления</param>
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

        /// <summary>
        /// Создание поля для ввода по соответствующему тэгу
        /// </summary>
        /// <param name="tag">Тэг, по которому формируется поле для ввода</param>
        /// <returns>Контрол поля для ввода значений</returns>
        private Control? FieldControlByDataType(Tag tag)
        {
            return ScriptGenControlsStaticMethods.FieldControlByDataType(tag);
        }

        /// <summary>
        /// Получение значения выбранного в поле
        /// </summary>
        /// <param name="control">Контрол из которого нужно получить значение</param>
        /// <returns>Название параметра и значение параметра</returns>
        private KeyValuePair<string, string>? GetControlValue(Control? control)
        {
            return ScriptGenControlsStaticMethods.GetControlValue(control);
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