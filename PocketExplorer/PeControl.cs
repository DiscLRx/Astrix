using PocketExplorer.Data;
using System.Collections.ObjectModel;

namespace PocketExplorer;

public class PeControl
{
    private PeDataControl _dataControl;

    private Dictionary<int, PeHost> _peHosts = [];

    public PeControl()
    {
        _dataControl = new();
    }

    public bool IsPortUnique(int port)
        => _dataControl.IsPortUnique(port);

    public void AddInstance(int port)
        => _dataControl.AddInstance(new(port, []));

    public void RemoveInstance(int port)
    {
        _dataControl.RemoveInstance(port);
    }

    public PeInstance GetInstance(int port)
        => _dataControl.GetInstance(port);

    public ObservableCollection<PeInstance> GetInstances()
        => _dataControl.Data.Instances;

    public ObservableCollection<int> GetPorts()
        => [.. _dataControl.Data.Instances.Select(inst => inst.Port)];

    public bool IsLocationNameUnique(int port, string locationName)
        => _dataControl.IsLocationNameUnique(port, locationName);

    public void AddLocation(int instancePort, PeLocation locationInfo)
        => _dataControl.AddLocation(instancePort, locationInfo);

    public void RemoveLocation(int instancePort, string locationName)
        => _dataControl.RemoveLocation(instancePort, locationName);

    public PeLocation GetLocation(int instancePort, string name)
        => _dataControl.GetLocation(instancePort, name);

    public ObservableCollection<PeLocation> GetLocations(int instancePort)
        => _dataControl.GetInstance(instancePort).Locations;

    public async Task StartInstanceAsync(int port)
    {
        if(NetState.IsPortUsing(port))
        {
            throw new Exception($"Port {port} is used.");
        }
        var instance = GetInstance(port);
        var host = new PeHost(instance);
        _peHosts.Add(port, host);
        await host.StartAsync();
        _dataControl.GetInstance(port).Enable = true;
    }

    public async Task StopInstanceAsync(int port)
    {
        var host = _peHosts[port];
        await host.StopAsync();
        _dataControl.GetInstance(port).Enable = false;
        _peHosts.Remove(port);
    }

    public void SetInstanceLocked(PeInstance instance, bool isLocked) 
        => _dataControl.SetInstanceLocked(instance, isLocked);

    public void SetInstancePassword(PeInstance instance, string password) 
        => _dataControl.SetInstancePassword(instance, password);

}