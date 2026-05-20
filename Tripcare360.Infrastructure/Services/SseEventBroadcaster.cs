using System.Collections.Concurrent;
using System.Text.Json;
using Tripcare360.Application.Interfaces.Services;

namespace Tripcare360.Infrastructure.Services;

public interface ISseEventBroadcaster
{
    void RegisterClient(string claimCode, Func<string, string, Task> onEvent);
    void UnregisterClient(string claimCode);
    Task BroadcastStateAsync(string claimCode, string eventType, object data);
}

public class SseEventBroadcaster : ISseEventBroadcaster
{
    private readonly ConcurrentDictionary<string, Func<string, string, Task>> _activeConnections = new();

    public void RegisterClient(string claimCode, Func<string, string, Task> onEvent) =>
        _activeConnections[claimCode] = onEvent;

    public void UnregisterClient(string claimCode) =>
        _activeConnections.TryRemove(claimCode, out _);

    public async Task BroadcastStateAsync(string claimCode, string eventType, object data)
    {
        if (_activeConnections.TryGetValue(claimCode, out var writeDelegate))
        {
            var jsonPayload = JsonSerializer.Serialize(data);
            await writeDelegate(eventType, jsonPayload);
        }
    }
}
