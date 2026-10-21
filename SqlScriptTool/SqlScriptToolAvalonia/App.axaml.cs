using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SqlScriptToolClient.ConstructorStorage;

namespace SqlScriptToolAvalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
            desktop.Startup += OnStartup;
            desktop.Exit += OnExit;
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    private void OnStartup(object s, ControlledApplicationLifetimeStartupEventArgs e)
    {
        StaticConstructorStorage.InitializeStorage();
        StaticConstructorStorage.LoadStorage();
    }
    private void OnExit(object sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        StaticConstructorStorage.SaveStorage();
    }
}