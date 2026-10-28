using Avalonia.Controls;
using Avalonia.Interactivity;

namespace SqlScriptToolAvalonia.AdditionalWindows;

/// <summary>
/// Логика взаимодействия для SettingsWindow.axaml.
/// Перенос формы и логики из SqlScriptToolClient.SettingsWindow (WPF)
/// </summary>
public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    private void CheckBox_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is CheckBox checkBox)
        {
            if (checkBox.IsChecked.HasValue && checkBox.IsChecked.Value)
            {
                PwdTextBox.Text = PwdBoxInput.Password; // скопируем в TextBox из PasswordBox
                PwdTextBox.IsVisible = true; // TextBox - отобразить
                PwdBoxInput.IsVisible = false; // PasswordBox - скрыть
            }
            else
            {
                PwdBoxInput.Password = PwdTextBox.Text; // скопируем в PasswordBox из TextBox
                PwdTextBox.IsVisible = false; // TextBox - скрыть
                PwdBoxInput.IsVisible = true; // PasswordBox - отобразить
            }
        }
    }

    private void BtnEdit_Click(object? sender, RoutedEventArgs e)
    {
    }

    private void BtnOk_Click(object? sender, RoutedEventArgs e)
    {
    }

    private void BtnTest_Click(object? sender, RoutedEventArgs e)
    {
    }

    private void BtnCancel_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void BtnChange_Click(object? sender, RoutedEventArgs e)
    {
        ConnectionWindow connectionWindow = new ConnectionWindow();
        connectionWindow.Show();
    }
}