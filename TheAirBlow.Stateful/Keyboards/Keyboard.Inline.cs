using Telegram.Bot.Types.ReplyMarkups;

namespace TheAirBlow.Stateful.Keyboards;

/// <summary>
/// A simpler inline keyboard
/// </summary>
public static partial class Keyboard {
    /// <summary>
    /// Creates a new inline keyboard from a list of button names.
    /// To make the next button appear from a new line, add a newline at the end.
    /// </summary>
    /// <param name="buttons">List of buttons</param>
    /// <returns>Inline keyboard markup</returns>
    public static InlineKeyboardMarkup Inline(params string[] buttons) {
        var list = new List<List<InlineKeyboardButton>>();
        var current = new List<InlineKeyboardButton>();
        list.Add(current);
        foreach (var button in buttons) {
            current.Add(new InlineKeyboardButton(button.TrimEnd('\n')) 
                { CallbackData = button.TrimEnd('\n') });

            if (!button.EndsWith('\n')) continue;
            current = []; list.Add(current);
        }
        
        return new InlineKeyboardMarkup(list);
    }
    
    /// <summary>
    /// Creates a new inline keyboard from a list of button names.
    /// To make the next button appear from a new line, add a newline at the end.
    /// </summary>
    /// <param name="buttons">List of buttons</param>
    /// <returns>Inline keyboard markup</returns>
    public static InlineKeyboardMarkup Inline(Dictionary<string, string> buttons) {
        var list = new List<List<InlineKeyboardButton>>();
        var current = new List<InlineKeyboardButton>();
        list.Add(current);
        foreach (var button in buttons) {
            current.Add(new InlineKeyboardButton(button.Key.TrimEnd('\n')) 
                { CallbackData = button.Value });

            if (!button.Key.EndsWith('\n')) continue;
            current = []; list.Add(current);
        }

        return new InlineKeyboardMarkup(list);
    }
}