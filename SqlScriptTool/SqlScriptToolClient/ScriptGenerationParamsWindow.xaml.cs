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
using SqlScriptToolClient.CustomComponents;
using SSTCommonClasses.Models;

namespace SqlScriptToolClient
{
    /// <summary>
    /// Логика взаимодействия для ScriptGenerationParamsWindow.xaml
    /// </summary>
    public partial class ScriptGenerationParamsWindow : Window
    {
        public Model SelectedModel { get; set; }
        

        public ScriptGenerationParamsWindow(Model model)
        {
            InitializeComponent();
            SelectedModel = model;
            var sqlTemplate = SelectedModel.RootModelItem.SqlTemplate;
            MultiplePatternControl multiplePatternControl = new MultiplePatternControl(sqlTemplate);
            SpParamsList.Children.Add(multiplePatternControl);
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnGenScript_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in SpParamsList.Children)
            {
                if (child is MultiplePatternControl multiplePatternControl)
                {
                    MessageBox.Show(multiplePatternControl.MultiplePatternsText);
                }
            }
        }
    }
}
