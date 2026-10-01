using System.Text;
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
            var data = button.TrimEnd('\n');
            current.Add(new InlineKeyboardButton(data) 
                { CallbackData = ValidateData(data, data) });

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
    public static InlineKeyboardMarkup Inline(IEnumerable<KeyValuePair<string, string>> buttons) {
        var list = new List<List<InlineKeyboardButton>>();
        var current = new List<InlineKeyboardButton>();
        list.Add(current);
        foreach (var button in buttons) {
            current.Add(new InlineKeyboardButton(button.Key.TrimEnd('\n')) 
                { CallbackData = ValidateData(button.Key, button.Value) });

            if (!button.Key.EndsWith('\n')) continue;
            current = []; list.Add(current);
        }

        return new InlineKeyboardMarkup(list);
    }

    /// <summary>
    /// Checks that callback data is valid for Telegram (1-64 bytes)
    /// </summary>
    /// <param name="button">Button name, for the error message</param>
    /// <param name="data">Callback data</param>
    /// <returns>Callback data</returns>
    private static string ValidateData(string button, string data) {
        var length = Encoding.UTF8.GetByteCount(data);
        if (length is < 1 or > 64)
            throw new ArgumentException($"Callback data of button \"{button.TrimEnd('\n')}\" is {length} bytes, Telegram requires 1 to 64");
        return data;
    }
}