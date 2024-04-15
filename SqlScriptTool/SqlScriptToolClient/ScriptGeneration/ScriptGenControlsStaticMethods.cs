using System;
using System.Windows.Controls;
using System.Windows;
using Xceed.Wpf.Toolkit;
using System.Collections.Generic;
using SqlScriptToolClient.CustomComponents;
using SSTCommonClasses.Constructor.Models;
using SSTCommonClasses.Constructor.Tags;

namespace SqlScriptToolClient.ScriptGeneration;

/// <summary>
/// Статический класс с методами для обработки пользовательских параметров
/// </summary>
public static class ScriptGenControlsStaticMethods
{
    /// <summary>
    /// Создание поля для ввода по соответствующему тэгу
    /// </summary>
    /// <param name="tag">Тэг, по которому формируется поле для ввода</param>
    /// <returns>Контрол поля для ввода значений</returns>
    public static Control? FieldControlByDataType(Tag tag)
    {
        Control? result = null;
        TagDataType? dataType = tag.DataType;
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
            case TagDataType.Custom:
                ComboBox ldObjectComboBox = new ComboBox();
                result = ldObjectComboBox;
                break;
            case TagDataType.String:
                TextBox textBox = new TextBox();
                result = textBox;
                break;
        }

        if (result != null)
        {
            result.Tag = tag;
        }

        return result;
    }

    /// <summary>
    /// Получение значения выбранного в поле
    /// </summary>
    /// <param name="control">Контрол из которого нужно получить значение</param>
    /// <returns>Название параметра и значение параметра</returns>
    public static KeyValuePair<string, string>? GetControlValue(Control? control)
    {
        KeyValuePair<string, string>? result = null;
        if (control == null)
        {
            return null;
        }

        if (control.Tag is Tag tag)
        {
            string tagName = tag.Name;
            TagDataType? dataType = tag.DataType;
            if (dataType == null)
            {
                return null;
            }

            string? controlValue = null;
            switch (dataType)
            {
                case TagDataType.Char:
                    if (control is CheckBox checkBox)
                    {
                        var isChecked = checkBox.IsChecked;
                        controlValue = isChecked.HasValue && isChecked.Value ? "+" : "-";
                    }


                    break;
                case TagDataType.Date:
                    if (control is DateTimePicker datePickerControl)
                    {
                        DateTime? datePicker = datePickerControl.Value;
                        controlValue = datePicker.HasValue
                            ? datePicker.Value.ToString("yyyyMMdd")
                            : string.Empty;
                    }

                    break;
                case TagDataType.DateTime:
                    if (control is DateTimePicker dateTimePickerControl)
                    {
                        DateTime? dateTimePicker = dateTimePickerControl.Value;
                        controlValue = dateTimePicker.HasValue
                            ? dateTimePicker.Value.ToString("yyyy-MM-dd HH:mm:ss")
                            : string.Empty;
                    }

                    break;
                case TagDataType.Double:
                    if (control is DoubleUpDown doubleUpDownControl)
                    {
                        double? doubleControl = doubleUpDownControl.Value;
                        controlValue = doubleControl.HasValue
                            ? doubleControl.Value.ToString("R")
                            : "0";
                    }

                    break;
                case TagDataType.Int:
                    if (control is IntegerUpDown integerUpDownControl)
                    {
                        int? intControl = integerUpDownControl.Value;
                        controlValue = intControl.HasValue ? intControl.Value.ToString() : "0";
                    }

                    break;
                case TagDataType.Custom:
                    controlValue = "0";
                    break;
                case TagDataType.String:
                    if (control is TextBox textBoxControl)
                    {
                        controlValue = textBoxControl.Text;
                    }

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

        if (control.Tag is SqlTemplate sqlTemplate)
        {
            if (!sqlTemplate.IsMultiplePattern)
            {
                return null;
            }

            if (control is MultiplePatternControl multiplePatternControl)
            {
                string controlText = multiplePatternControl.MultiplePatternsText;
                result = new KeyValuePair<string, string>(sqlTemplate.Name, controlText);
            }
        }

        return result;
    }
}