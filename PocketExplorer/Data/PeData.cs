using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PocketExplorer.Data;

public class PeLocation(string name, string path) : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    }

    public string Name
    {
        get;
        set
        {
            field = value;
            OnChanged();
        }
    } = name;

    public string Path
    {
        get;
        set
        {
            field = value;
            OnChanged();
        }
    } = path;
}

public class PeInstance(int port, ObservableCollection<PeLocation> locations) : INotifyPropertyChanged, INotifyCollectionChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    protected void OnPropertyChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    }

    public int Port
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = port;

    public ObservableCollection<PeLocation> Locations
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = locations;

    [JsonIgnore]
    public bool Enable
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = false;

    public void AddLocation(PeLocation location)
    {
        Locations.Add(location);
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, location));
    }

    public void RemoveLocation(string name)
    {
        var index = Locations.ToList().FindIndex(lc => lc.Name == name);
        Locations.RemoveAt(index);
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, null));
    }
}

public class PeStatus(ObservableCollection<PeInstance> instances) : INotifyPropertyChanged, INotifyCollectionChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public ObservableCollection<PeInstance> Instances
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
    } = instances;
    public void AddInstance(PeInstance instance)
    {
        Instances.Add(instance);
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, instance));
    }
    public void RemoveInstance(int port)
    {
        var index = Instances.ToList().FindIndex(inst => inst.Port == port);
        Instances.RemoveAt(index);
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, null));
    }
}


