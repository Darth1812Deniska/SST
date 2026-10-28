using System.Threading.Tasks;
using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace SqlScriptToolAvalonia.Helpers;

/// <summary>
/// Вспомогательный класс, предоставляющий методы вывода сообщений
/// (аналог WPF MessageBox на базе MessageBox.Avalonia)
/// </summary>
public static class AvaloniaMessageBox
{
    /// <summary>
    /// Показать окно с сообщением и кнопкой OK
    /// </summary>
    public static void Show(string message, string title = "")
    {
        MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok).ShowAsync();
    }

    /// <summary>
    /// Показать модальное окно с сообщением. Возвращает true, если нажата кнопка OK, иначе false
    /// </summary>
    public static async Task<bool> ShowOkCancelAsync(Window owner, string message, string title = "")
    {
        var msBox = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.OkCancel, Icon.Warning);
        ButtonResult result = await msBox.ShowWindowDialogAsync(owner);
        return result == ButtonResult.Ok;
    }

    /// <summary>
    /// Показать модальное окно с вопросом. Возвращает результат: Yes / No / Cancel / None
    /// </summary>
    public static async Task<ButtonResult> ShowQuestionAsync(Window owner, string message, string title = "",
        ButtonEnum buttons = ButtonEnum.YesNo)
    {
        var msBox = MessageBoxManager.GetMessageBoxStandard(title, message, buttons, Icon.Question);
        return await msBox.ShowWindowDialogAsync(owner);
    }

    /// <summary>
    /// Показать окно с сообщением об ошибке
    /// </summary>
    public static void ShowError(string message, string title = "Ошибка")
    {
        MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, Icon.Error).ShowAsync();
    }
}
