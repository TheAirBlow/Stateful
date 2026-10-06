using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
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
    /// When should the state handler delete this message state, null to keep it.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
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
    /// Returns the stored JSON of a state value
    /// </summary>
    /// <param name="key">Dictionary Key</param>
    /// <returns>JSON, null if there is none</returns>
    private string? Find(string key)
        => LocalState.TryGetValue(key, out var local) ? local : State.GetValueOrDefault(key);

    /// <summary>
    /// Get state value using reflection. Not trimming or AOT safe, use the overload with type info instead.
    /// </summary>
    /// <param name="key">Dictionary Key</param>
    /// <typeparam name="T">Type</typeparam>
    /// <returns>Value of type</returns>
    [RequiresUnreferencedCode("Serializes using reflection, pass a JsonTypeInfo instead")]
    [RequiresDynamicCode("Serializes using reflection, pass a JsonTypeInfo instead")]
    public T? GetState<T>(string key)
        => Find(key) is { } json ? JsonSerializer.Deserialize<T>(json) : default;

    /// <summary>
    /// Get state value
    /// </summary>
    /// <param name="key">Dictionary Key</param>
    /// <param name="typeInfo">Type info of the value</param>
    /// <typeparam name="T">Type</typeparam>
    /// <returns>Value of type</returns>
    public T? GetState<T>(string key, JsonTypeInfo<T> typeInfo)
        => Find(key) is { } json ? JsonSerializer.Deserialize(json, typeInfo) : default;
    
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
    /// Set state value using reflection. Not trimming or AOT safe, use the overload with type info instead.
    /// </summary>
    /// <param name="key">Dictionary Key</param>
    /// <param name="value">Dictionary Value</param>
    /// <param name="local">Keep the value on this message only instead of passing it on to the next messages.
    /// When used before sending a message, the value is attached to the sent message.</param>
    [RequiresUnreferencedCode("Serializes using reflection, pass a JsonTypeInfo instead")]
    [RequiresDynamicCode("Serializes using reflection, pass a JsonTypeInfo instead")]
    public void SetState(string key, object value, bool local = false)
        => Store(key, JsonSerializer.Serialize(value), local);

    /// <summary>
    /// Set state value
    /// </summary>
    /// <param name="key">Dictionary Key</param>
    /// <param name="value">Dictionary Value</param>
    /// <param name="typeInfo">Type info of the value</param>
    /// <param name="local">Keep the value on this message only instead of passing it on to the next messages.
    /// When used before sending a message, the value is attached to the sent message.</param>
    /// <typeparam name="T">Type</typeparam>
    public void SetState<T>(string key, T value, JsonTypeInfo<T> typeInfo, bool local = false)
        => Store(key, JsonSerializer.Serialize(value, typeInfo), local);

    /// <summary>
    /// Stores the JSON of a state value
    /// </summary>
    /// <param name="key">Dictionary Key</param>
    /// <param name="json">JSON</param>
    /// <param name="local">Keep the value on this message only</param>
    private void Store(string key, string json, bool local) {
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
