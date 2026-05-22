namespace Tripcare360.Application.Interfaces.Services;

public interface ISseEventBroadcaster
{
    void RegisterClient(string claimCode, Func<string, string, Task> onEvent);
    void UnregisterClient(string claimCode);
    Task BroadcastStateAsync(string claimCode, string eventType, object data);
}
