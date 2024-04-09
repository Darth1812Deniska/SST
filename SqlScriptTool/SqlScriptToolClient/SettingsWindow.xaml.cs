using System.Windows;
using System.Windows.Controls;

namespace SqlScriptToolClient
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                if (checkBox.IsChecked.HasValue && checkBox.IsChecked.Value)
                {
                    PwdTextBox.Text = PwdPasswordBox.Password; // скопируем в TextBox из PasswordBox
                    PwdTextBox.Visibility = Visibility.Visible; // TextBox - отобразить
                    PwdPasswordBox.Visibility = Visibility.Hidden; // PasswordBox - скрыть
                }
                else
                {
                    PwdPasswordBox.Password = PwdTextBox.Text; // скопируем в PasswordBox из TextBox 
                    PwdTextBox.Visibility = Visibility.Hidden; // TextBox - скрыть
                    PwdPasswordBox.Visibility = Visibility.Visible; // PasswordBox - отобразить
                }
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnChange_Click(object sender, RoutedEventArgs e)
        {
            ConnectionWindow connectionWindow = new ConnectionWindow();
            connectionWindow.Show();
        }
    }
}
