using System.Text.Json;
using JetBrains.Annotations;

namespace TheAirBlow.Stateful;

/// <summary>
/// Message state information
/// </summary>
[PublicAPI]
public class MessageState {
    /// <summary>
    /// Empty message state.
    /// </summary>
    public static MessageState None => new();
    
    /// <summary>
    /// A dictionary of states you can use to store arbitrary information
    /// </summary>
    public Dictionary<string, string> State { get; set; } = [];
    
    /// <summary>
    /// A dictionary of states that belong to this message only.
    /// </summary>
    public Dictionary<string, string> LocalState { get; set; } = [];
    
    /// <summary>
    /// Set by the state handler when a clicked message has no stored state
    /// </summary>
    public bool Expired { get; set; }
    
    /// <summary>
    /// Increased on every update, used to detect concurrent modifications
    /// </summary>
    public int Version { get; set; }
    
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
    /// Local state keys that were set for a message that is not sent yet
    /// </summary>
    internal HashSet<string> PendingLocal { get; } = [];
    
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
        => LocalState.TryGetValue(key, out var local) ? JsonSerializer.Deserialize<T>(local)
            : State.TryGetValue(key, out var value) ? JsonSerializer.Deserialize<T>(value)
            : default;
    
    /// <summary>
    /// Remove state
    /// </summary>
    /// <param name="key">Dictionary Key</param>
    public void RemoveState(string key) {
        LastUpdated = DateTime.UtcNow;
        State.Remove(key);
        LocalState.Remove(key);
        PendingLocal.Remove(key);
    }
    
    /// <summary>
    /// Set state value
    /// </summary>
    /// <param name="key">Dictionary Key</param>
    /// <param name="value">Dictionary Value</param>
    /// <param name="local">Keep the value on this message only instead of passing it on to the next messages.
    /// When used before sending a message, the value is attached to the sent message.</param>
    public void SetState(string key, object value, bool local = false) {
        var json = JsonSerializer.Serialize(value);
        LastUpdated = DateTime.UtcNow;
        if (local) {
            State.Remove(key);
            LocalState[key] = json;
            PendingLocal.Add(key);
            return;
        }

        LocalState.Remove(key);
        PendingLocal.Remove(key);
        State[key] = json;
    }
    
    /// <summary>
    /// Completely clear state
    /// </summary>
    public void ClearState() {
        LastUpdated = DateTime.UtcNow;
        State.Clear();
        LocalState.Clear();
        PendingLocal.Clear();
    }
}
