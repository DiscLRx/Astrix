using System.Text.Json;

namespace PocketExplorer.Data;

public class BasePeDataControl
{
    private const string DataFilePath = "data/pe-data.json";

    private PeStatus _data;

    public PeStatus Data { get => _data; }

    protected BasePeDataControl()
    {
        var dataStr = File.ReadAllText(DataFilePath);
        _data = JsonSerializer.Deserialize<PeStatus>(dataStr)
            ?? throw new Exception("Failed to load pocket explorer data file");
    }

    public void SaveChange()
    {
        var dataStr = JsonSerializer.Serialize(_data);
        File.WriteAllText(DataFilePath, dataStr);
    }

}

public class PeDataControl : BasePeDataControl
{
    public bool IsPortUnique(int port)
        => !Data.Instances.Any(inst => inst.Port == port);

    public void AddInstance(PeInstance instance)
    {
        Data.AddInstance(instance);
        SaveChange();
    }

    public void RemoveInstance(int port)
    {
        Data.RemoveInstance(port);
        SaveChange();
    }

    public PeInstance GetInstance(int port)
        => Data.Instances.Single(inst => inst.Port == port);

    public bool IsLocationNameUnique(int port, string locationName)
    {
        var locations = Data.Instances.Single(inst => inst.Port == port).Locations;
        return !locations.Any(lc => lc.Name == locationName);
    }

    public void AddLocation(int instancePort, PeLocation locationInfo)
    {
        Data.Instances.Single(inst => inst.Port == instancePort)
            .Locations.Add(locationInfo);
        SaveChange();
    }

    public void RemoveLocation(int instancePort, string locationName)
    {
        var locations = Data.Instances.Single(inst => inst.Port == instancePort).Locations;
        var index = locations.ToList().FindIndex(lc => lc.Name == locationName);
        locations.RemoveAt(index);
        SaveChange();
    }

    public PeLocation GetLocation(int instancePort, string name)
        => Data.Instances.Single(inst => inst.Port == instancePort)
        .Locations.Single(lc => lc.Name == name);
}