using System.Diagnostics.CodeAnalysis;
using Telegram.Bot.Types.ReplyMarkups;

namespace Stateful.Keyboards;

/// <summary>
/// A simpler reply keyboard
/// </summary>
public static partial class Keyboard {
    /// <summary>
    /// Creates a new reply keyboard from a list of button names.
    /// To make the next button appear from a new line, add a newline at the end.
    /// </summary>
    /// <param name="buttons">List of buttons</param>
    /// <returns>Reply keyboard markup</returns>
    public static ReplyKeyboardMarkup Reply(params string[] buttons) {
        var list = new List<List<KeyboardButton>>();
        var current = new List<KeyboardButton>();
        list.Add(current);
        foreach (var button in buttons) {
            current.Add(new KeyboardButton(button.TrimEnd('\n')));
            if (!button.EndsWith('\n')) continue;
            current = []; list.Add(current);
        }
        
        return new ReplyKeyboardMarkup(list) { ResizeKeyboard = true };
    }
}