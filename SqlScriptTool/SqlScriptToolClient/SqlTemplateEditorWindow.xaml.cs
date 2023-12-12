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
using SSTCommonClasses.Models;

namespace SqlScriptToolClient
{
    /// <summary>
    /// Логика взаимодействия для SqlTemplateEditorWindow.xaml
    /// </summary>
    public partial class SqlTemplateEditorWindow : Window
    {
        public SqlTemplate SqlTemplate { get; }
        public ModelItem ModelItem { get; }

        public SqlTemplateEditorWindow()
        {
            InitializeComponent();
        }
        public SqlTemplateEditorWindow(ModelItem modelItem)
        {
            ModelItem = modelItem;
            InitializeComponent();
            MessageBox.Show(ModelItem.Uuid.ToString());
        }
        public SqlTemplateEditorWindow(SqlTemplate sqlTemplate)
        {
            SqlTemplate = sqlTemplate;
            InitializeComponent();
        }
    }
}
