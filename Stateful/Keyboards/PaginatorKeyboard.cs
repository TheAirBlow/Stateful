using System.Diagnostics.CodeAnalysis;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Stateful.Keyboards;

/// <summary>
/// Paginator keyboard
/// </summary>
public static partial class Keyboard {
    /// <summary>
    /// Creates a new paginated inline keyboard from a list of button names.
    /// To make the next button appear from a new line, add a newline at the end.
    /// </summary>
    /// <param name="handler">Update handler</param>
    /// <param name="perPage">Options per page</param>
    /// <param name="buttons">List of buttons</param>
    /// <returns>Inline keyboard markup</returns>
    public static InlineKeyboardMarkup Paginator(UpdateHandler handler, int perPage = 5, params string[] buttons)
        => Inline(GeneratePaginator(handler, perPage, buttons.ToDictionary(x => x, x => x), new Dictionary<string, string>()));

    /// <summary>
    /// Creates a new paginated inline keyboard from a list of button names.
    /// To make the next button appear from a new line, add a newline at the end.
    /// </summary>
    /// <param name="handler">Update handler</param>
    /// <param name="perPage">Options per page</param>
    /// <param name="buttons">Button dictionary</param>
    /// <param name="extra">Extra buttons</param>
    /// <returns>Inline keyboard markup</returns>
    public static InlineKeyboardMarkup Paginator(UpdateHandler handler, Dictionary<string, string> buttons, int perPage = 5, Dictionary<string, string>? extra = null) 
        => Inline(GeneratePaginator(handler, perPage, buttons, extra ?? new Dictionary<string, string>()));

    /// <summary>
    /// Generates a paginator keyboard
    /// </summary>
    /// <param name="handler">Update handler</param>
    /// <param name="perPage">Options per page</param>
    /// <param name="buttons">Buttons</param>
    /// <param name="extra">Extra</param>
    /// <returns>Paginator keyboard</returns>
    private static Dictionary<string, string> GeneratePaginator(UpdateHandler handler, 
        int perPage, Dictionary<string, string> buttons, Dictionary<string, string> extra) {
        var data = new PaginatorData {
            Buttons = buttons, Extra = extra,
            PerPage = perPage, Page = 0
        };
        handler.State.SetState("paginator_data", data);
        return data.GetButtons();
    }

    /// <summary>
    /// Paginator data
    /// </summary>
    internal class PaginatorData {
        /// <summary>
        /// Dictionary of buttons
        /// </summary>
        public Dictionary<string, string> Buttons { get; set; } = new();
        
        /// <summary>
        /// Extra buttons to always show after pagination buttons
        /// </summary>
        public Dictionary<string, string> Extra { get; set; } = new();

        /// <summary>
        /// How many pages in total
        /// </summary>
        public int Pages => (int)Math.Ceiling(Buttons.Count / (float)PerPage);
        
        /// <summary>
        /// How many buttons per page
        /// </summary>
        public int PerPage { get; set; }
        
        /// <summary>
        /// Current page starting from zero
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Returns all buttons together with pagination stuff
        /// </summary>
        /// <returns>Buttons dictionary</returns>
        public Dictionary<string, string> GetButtons() {
            var buttons = Buttons.Skip(PerPage * Page).Take(PerPage)
                .ToDictionary(button => button.Key, button => button.Value);
            if (Pages > 1) for (var i = 1; i <= Pages; i++)
                buttons.Add((Page + 1 == i ? $"· {i} ·" : $"{i}") + (i == Pages || i % 8 == 0 ? "\n" : ""), $"stinternal-paginator-{i-1}");
            foreach (var button in Extra)
                buttons.Add(button.Key, button.Value);
            return buttons;
        }
    }
}