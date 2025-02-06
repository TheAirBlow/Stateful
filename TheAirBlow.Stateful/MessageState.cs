using System.Text.Json;
using JetBrains.Annotations;

namespace TheAirBlow.Stateful;

/// <summary>
/// Message state information
/// </summary>
[PublicAPI]
public class MessageState {
    /// <summary>
    /// Null message state
    /// </summary>
    public static readonly MessageState None = new();
    
    /// <summary>
    /// A dictionary of states you can use to store arbitrary information
    /// </summary>
    public Dictionary<string, string> State { get; set; } = [];
    
    /// <summary>
    /// When was this message state last updated
    /// </summary>
    public DateTime LastUpdated { get; set; }
    
    /// <summary>
    /// Unique identifier of the current update handler
    /// </summary>
    public string? HandlerId { get; set; }
    
    /// <summary>
    /// Arbitrary submenu value
    /// </summary>
    public string? SubMenu { get; set; }
    
    /// <summary>
    /// Telegram message ID
    /// </summary>
    public long MessageId { get; set; }
    
    /// <summary>
    /// Telegram chat ID
    /// </summary>
    public long ChatId { get; set; }
    
    /// <summary>
    /// Changes Handler ID
    /// </summary>
    /// <param name="id">Handler ID</param>
    internal void SetHandler(string? id) {
        LastUpdated = DateTime.UtcNow;
        HandlerId = id;
    }

    /// <summary>
    /// Get state value
    /// </summary>
    /// <param name="key">Dictionary Key</param>
    /// <typeparam name="T">Type</typeparam>
    /// <returns>Value of type</returns>
    public T? GetState<T>(string key)
        => !State.TryGetValue(key, out var value)
            ? default : JsonSerializer.Deserialize<T>(value);
    
    /// <summary>
    /// Remove state
    /// </summary>
    /// <param name="key">Dictionary Key</param>
    public void RemoveState(string key) {
        LastUpdated = DateTime.Now;
        State.Remove(key);
    }
    
    /// <summary>
    /// Set state value
    /// </summary>
    /// <param name="key">Dictionary Key</param>
    /// <param name="value">Dictionary Value</param>
    public void SetState(string key, object value) {
        State[key] = JsonSerializer.Serialize(value);
        LastUpdated = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Completely clear state
    /// </summary>
    public void ClearState() {
        LastUpdated = DateTime.UtcNow;
        State.Clear();
    }
}