using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using SSTCommonClasses.ConstructorStorage;

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

        // Подписка на ошибки загрузки хранилища: бизнес-логика (SSTCommonClasses) не знает про UI,
        // а UI показывает накопленные ошибки пользователю.
        StaticConstructorStorage.LoadErrorsOccurred += OnLoadErrorsOccurred;

        base.OnFrameworkInitializationCompleted();
    }
    
    private static void OnLoadErrorsOccurred(string message)
    {
        MessageBoxManager.GetMessageBoxStandard("Ошибка!",
                message, ButtonEnum.Ok, Icon.Error,
                WindowStartupLocation.CenterScreen).ShowAsync();
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
