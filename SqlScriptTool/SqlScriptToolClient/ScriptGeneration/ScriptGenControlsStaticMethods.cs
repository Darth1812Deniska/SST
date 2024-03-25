using System.Windows.Controls;
using System.Windows;
using Xceed.Wpf.Toolkit;
using System.Collections.Generic;
using System;
using SSTCommonClasses.Constructor.Tags;

namespace SqlScriptToolClient.ScriptGeneration;

public static class ScriptGenControlsStaticMethods
{
    public static Control? FieldControlByDataType(Tag tag)
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

        if (result != null)
        {
            result.Tag = tag;
        }
        return result;
    }

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
}