using SqlScriptToolClient.Connection;
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

namespace SqlScriptToolClient
{
    /// <summary>
    /// Логика взаимодействия для ConnectionWindow.xaml
    /// </summary>
    public partial class ConnectionWindow : Window
    {
        public ConnectionWindow()
        {
            InitializeComponent();
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (port is null)
            {
                return;
            }

            switch (((ComboBoxItem)((ComboBox)sender).SelectedItem).Content.ToString())
            {
                case "PostgreSQL":
                    {
                        port.Text = "5432";
                        port.IsReadOnly = true;
                        break;
                    }
                case "MSSQL":
                    {
                        port.IsReadOnly = false;
                        port.Text = "3306";
                        break;
                    }
            }
        }

        private void port_Initialized(object sender, EventArgs e)
        {
            //Микрокостыль, первое событие вызывается еще до того, как инициализируется поле port
            //Можно конечно таску заставить ждать, но так будет быстрее
            ComboBox_SelectionChanged(DBType, null);
        }

        private void CheckConnection()
        {
            ConnectionChecker connectionChecker = new ConnectionChecker();
            string connectionString = string.Empty;

            switch (((ComboBoxItem)DBType.SelectedItem).Content) 
            {
                case "PostgreSQL":
                    {
                        connectionString = $"Server={address.Text};User Id={login.Text};Password={password.Password};Port={port.Text};Database={dbName.Text};";
                        connectionChecker = new Connection.PGConnectionChecker();
                        break;
                    }
                case "MSSQL":
                    {
                        connectionString = $"Persist Security Info=False;User ID={login};Password={password};Initial Catalog={dbName.Text};Server={address.Text},{port.Text};Encrypt=True;";

                        connectionChecker = new Connection.MSConnectionChecker();
                        break;
                    }
            }

            var result = connectionChecker.CheckConnection(connectionString);

            MessageBox.Show($"Подключение к БД {(result ? "успешно" : "не было установлено, проверьте введенные данные")}");


        }

        private void checkConnectionBtn_Click(object sender, RoutedEventArgs e)
        {
            CheckConnection();
        }
    }
}
