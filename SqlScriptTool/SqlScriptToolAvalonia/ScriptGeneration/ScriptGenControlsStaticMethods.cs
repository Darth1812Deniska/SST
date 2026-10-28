using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using SqlScriptToolAvalonia.CustomComponents;
using SSTCommonClasses.Constructor.Models;
using SSTCommonClasses.Constructor.Tags;

namespace SqlScriptToolAvalonia.ScriptGeneration;

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
                DatePicker datePicker = new DatePicker();
                result = datePicker;
                break;
            case TagDataType.DateTime:
                DatePicker dateTimePicker = new DatePicker();
                result = dateTimePicker;
                break;
            case TagDataType.Double:
                NumberBox doubleUpDown = new NumberBox()
                {
                    Width = 150,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
                };
                result = doubleUpDown;
                break;
            case TagDataType.Int:
                NumberBox intUpDown = new NumberBox()
                {
                    Width = 150,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                    // Целочисленный ввод: без дробной части и с шагом 1
                    SmallChange = 1,
                    LargeChange = 10,
                    FormatString = "0"
                };
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
                    if (control is DatePicker datePickerControl)
                    {
                        DateTime? datePicker = datePickerControl.SelectedDate.Value.DateTime;
                        controlValue = datePicker.HasValue
                            ? datePicker.Value.ToString("yyyyMMdd")
                            : string.Empty;
                    }

                    break;
                case TagDataType.DateTime:
                    if (control is DatePicker dateTimePickerControl)
                    {
                        DateTime? dateTimePicker = dateTimePickerControl.SelectedDate.Value.DateTime;
                        controlValue = dateTimePicker.HasValue
                            ? dateTimePicker.Value.ToString("yyyy-MM-dd HH:mm:ss")
                            : string.Empty;
                    }

                    break;
                case TagDataType.Double:
                    if (control is NumberBox doubleUpDownControl)
                    {
                        double? doubleControl = doubleUpDownControl.Value;
                        controlValue = doubleControl.HasValue
                            ? doubleControl.Value.ToString("R")
                            : "0";
                    }

                    break;
                case TagDataType.Int:
                    if (control is NumberBox integerUpDownControl)
                    {
                        double? intControl = integerUpDownControl.Value;
                        // Значение округляется до целого, как в исходном IntegerUpDown
                        controlValue = intControl.HasValue
                            ? Math.Round(intControl.Value).ToString()
                            : "0";
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