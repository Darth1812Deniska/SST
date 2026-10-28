using Avalonia.Controls;
using Avalonia.Interactivity;
using SqlScriptToolAvalonia.Helpers;
using SqlScriptToolClient.Connection;

namespace SqlScriptToolAvalonia.AdditionalWindows;

/// <summary>
/// Code-behind для ConnectionWindow.axaml.
/// Перенос логики из SqlScriptToolClient.ConnectionWindow (WPF)
/// </summary>
public partial class ConnectionWindow : Window
{
    public ConnectionWindow()
    {
        InitializeComponent();
    }

    private void ComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (TbPort is null)
        {
            return;
        }

        switch (((ComboBoxItem?)((ComboBox)sender!).SelectedItem)?.Content?.ToString())
        {
            case "PostgreSQL":
            {
                TbPort.Text = "5432";
                TbPort.IsReadOnly = true;
                break;
            }
            case "MSSQL":
            {
                TbPort.IsReadOnly = false;
                TbPort.Text = "3306";
                break;
            }
        }
    }

    private void CheckConnection()
    {
        ConnectionChecker connectionChecker = new ConnectionChecker();
        string connectionString = string.Empty;

        switch (((ComboBoxItem?)CmbDbType.SelectedItem)?.Content?.ToString())
        {
            case "PostgreSQL":
            {
                connectionString =
                    $"Server={CmbAddress.Text};" +
                    $"User Id={TbLogin.Text};" +
                    $"Password={PbPassInput.Text};" +
                    $"Port={TbPort.Text};" +
                    $"Database={TbDbName.Text};";
                connectionChecker = new PGConnectionChecker();
                break;
            }
            case "MSSQL":
            {
                connectionString =
                    "Persist Security Info=False;" +
                    $"User ID={TbLogin.Text};" +
                    $"Password={PbPassInput.Text};" +
                    $"Initial Catalog={TbDbName.Text};" +
                    $"Server={CmbAddress.Text},{TbPort.Text};" +
                    "Encrypt=True;";

                connectionChecker = new MSConnectionChecker();
                break;
            }
        }

        bool result = connectionChecker.CheckConnection(connectionString);

        AvaloniaMessageBox.Show(
            $"Подключение к БД {(result ? "успешно" : "не было установлено, проверьте введенные данные")}");
    }

    private void CheckConnectionBtn_Click(object? sender, RoutedEventArgs e)
    {
        CheckConnection();
    }

    private void BtnCancel_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
