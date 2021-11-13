namespace RemoteControl.HubLibrary;
public class RemoteControlHub : Hub
{
    private readonly static ConcurrentDictionary<string, string> _hosts = new();
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string group;
        bool rets = _hosts.TryRemove(Context.ConnectionId, out group!);
        if (rets)
        {
            await Clients.OthersInGroup(group).SendAsync("HostDisconnected");
        }
        await base.OnDisconnectedAsync(exception);
    }
    private static bool HasHost(string group)
    {
        return _hosts.Any(xx => xx.Value == group);
    }
    public async Task HostInitAsync(string group)
    {
        _hosts.TryAdd(Context.ConnectionId, group);
        await Clients.Caller.SendAsync("Hosting"); //host can decide what to do about hosting.
    }
    public async Task ClientInitAsync(string group)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, group);
        if (HasHost(group) == false)
        {
            await Clients.Caller.SendAsync("HostDisconnected");
        }
        else
        {
            string connectionid = _hosts.Single(xx => xx.Value == group).Key;
            await Clients.Client(connectionid).SendAsync("NewClient");
        }
    }
    public async Task ClientInvokeSimpleActionAsync(string group, string method)
    {
        if (HasHost(group) == false)
        {
            await Clients.Caller.SendAsync("Failed");
        }
        string connectionid = _hosts.Single(xx => xx.Value == group).Key;
        await Clients.Client(connectionid).SendAsync(method);
    }
    public async Task ClientInvokeComplexActionAsync(string group, string method, string payLoad) //since serializing does not work for this case.
    {
        if (HasHost(group) == false)
        {
            await Clients.Caller.SendAsync("Failed");
        }
        string connectionid = _hosts.Single(xx => xx.Value == group).Key;
        await Clients.Client(connectionid).SendAsync(method, payLoad);
    }
    public async Task HostSendClientDataAsync(string group, string method, string payLoad)
    {
        await Clients.OthersInGroup(group).SendAsync(method, payLoad);
    }
}