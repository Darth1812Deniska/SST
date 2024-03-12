using System.Windows;
using System.Windows.Controls;
using SqlScriptToolClient.ConstructorStorage;
using SSTCommonClasses.Models;

namespace SqlScriptToolClient;

/// <summary>
/// Логика взаимодействия для ScriptGenerationSelectingModelsWindow.xaml
/// </summary>
public partial class ScriptGenerationSelectingModelsWindow : Window
{
    public ScriptGenerationSelectingModelsWindow()
    {
        InitializeComponent();
        ShowModelList();
    }

    private void ShowModelList()
    {
        foreach (var model in StaticConstructorStorage.Models)
        {
            Button btn = new Button()
            {
                Content = model
            };
            btn.Click += Btn_Click;

            SpModelList.Children.Add(btn);
        }
    }

    private void Btn_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            if (button.Content is Model model)
            {
                ScriptGenerationParamsWindow wnd = new ScriptGenerationParamsWindow(model);
                wnd.Show();
                Close();
            }
        }
    }

    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}