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
    public async Task HostDisconnectAsync(string group)
    {
        if (_hosts.TryRemove(Context.ConnectionId, out _))
        {
            await Clients.OthersInGroup(group).SendAsync("HostDisconnected");
        }
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
            await SendToSeveralClientsAsync(group, "NewClient");
        }
    }
    private async Task SendToSeveralClientsAsync(string group, string method)
    {
        var list = _hosts.Where(xx => xx.Value == group);
        foreach (var item in list)
        {
            string connectionid = item.Key;
            await Clients.Client(connectionid).SendAsync(method);
        }
    }
    public async Task ClientInvokeSimpleActionAsync(string group, string method)
    {
        if (HasHost(group) == false)
        {
            await Clients.Caller.SendAsync("Failed");
        }
        //looks like i need to allow to send to more than one host (because its possible like for 2 music apps that needs sharing, needs to send both).

        await SendToSeveralClientsAsync(group, method);


        //string connectionid = _hosts.Single(xx => xx.Value == group).Key;

    }
    public async Task ClientInvokeComplexActionAsync(string group, string method, string payLoad) //since serializing does not work for this case.
    {
        if (HasHost(group) == false)
        {
            await Clients.Caller.SendAsync("Failed");
        }
        var list = _hosts.Where(xx => xx.Value == group);
        foreach (var item in list)
        {
            string connectionid = item.Key;
            await Clients.Client(connectionid).SendAsync(method, payLoad);
        }
        //await SendToSeveralClientsAsync(group, method);
        //string connectionid = _hosts.Single(xx => xx.Value == group).Key;
        //await Clients.Client(connectionid).SendAsync(method, payLoad);
    }
    public async Task HostSendClientDataAsync(string group, string method, string payLoad)
    {
        await Clients.OthersInGroup(group).SendAsync(method, payLoad);
    }
}