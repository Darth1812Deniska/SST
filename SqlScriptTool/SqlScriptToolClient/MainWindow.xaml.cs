using System.Windows;
using SqlScriptToolClient.ConstructorStorage;

namespace SqlScriptToolClient;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        LbModels.ItemsSource = StaticConstructorStorage.Models;
    }

    private void BtnCreateNewModel_Click(object sender, RoutedEventArgs e)
    {
        ModelEditorWindow modelEditorWindow = new ModelEditorWindow();
        modelEditorWindow.Show();
    }

    private void BtnCreateNewSqlTemplate_Click(object sender, RoutedEventArgs e)
    {
        SqlTemplateEditorWindow sqlTemplateEditorWindow = new SqlTemplateEditorWindow();
        sqlTemplateEditorWindow.Show();
    }

    private void BtnCreateNewTag_Click(object sender, RoutedEventArgs e)
    {
        TagEditorWindow tagEditorWindow = new TagEditorWindow();
        tagEditorWindow.Show();
    }
}