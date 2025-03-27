/// <summary>
/// Factory for creating a DeviceManager instance.
/// </summary>
public static class DeviceManagerFactory
{
    public static DeviceManager CreateDeviceManager()
    {
        return new DeviceManager(new FileDeviceStorage(), new CsvDeviceParser());
    }
}

/// <summary>
/// Interface for parsing devices from a file.
/// </summary>
public interface IDeviceParser
{
    List<Device> ParseDevices(string filePath);
}

public class CsvDeviceParser : IDeviceParser
{
    public List<Device> ParseDevices(string filePath)
    {
        // Implement CSV parsing logic
        // Example: Read file, parse CSV, create devices
        return new List<Device>();
    }
}

/// <summary>
/// Interface for storing and retrieving devices.
/// </summary>
public interface IDeviceStorage
{
    List<Device> LoadDevices();
}

public class FileDeviceStorage : IDeviceStorage
{
    public List<Device> LoadDevices()
    {
        // Implement file loading logic
        // Example: Read devices from a file
        return new List<Device>();
    }
}

/// <summary>
/// Manages a collection of electronic devices.
/// </summary>
public class DeviceManager
{
    private readonly IDeviceStorage _storage;
    private readonly IDeviceParser _parser;
    private readonly List<Device> _devices;

    public DeviceManager(IDeviceStorage storage, IDeviceParser parser)
    {
        _storage = storage;
        _parser = parser;
        _devices = _storage.LoadDevices();
    }

    public void AddDevice(Device device)
    {
        foreach (var storedDevice in _devices)
        {
            if (storedDevice.Id.Equals(device.Id))
            {
                throw new ArgumentException($"Device with ID {storedDevice.Id} is already stored.", nameof(device));
            }
        }

        
        _devices.Add(device);
    }

    public void EditDevice(Device editDevice)
    {
        var targetDeviceIndex = -1;
        for (var index = 0; index < _devices.Count; index++)
        {
            var storedDevice = _devices[index];
            if (storedDevice.Id.Equals(editDevice.Id))
            {
                targetDeviceIndex = index;
                break;
            }
        }

        if (targetDeviceIndex == -1)
        {
            throw new ArgumentException($"Device with ID {editDevice.Id} is not stored.", nameof(editDevice));
        }

        if (editDevice is Smartwatch)
        {
            if (_devices[targetDeviceIndex] is Smartwatch)
            {
                _devices[targetDeviceIndex] = editDevice;
            }
            else
            {
                throw new ArgumentException($"Type mismatch between devices. " +
                                            $"Target device has type {_devices[targetDeviceIndex].GetType().Name}");
            }
        }
        
        if (editDevice is PersonalComputer)
        {
            if (_devices[targetDeviceIndex] is PersonalComputer)
            {
                _devices[targetDeviceIndex] = editDevice;
            }
            else
            {
                throw new ArgumentException($"Type mismatch between devices. " +
                                            $"Target device has type {_devices[targetDeviceIndex].GetType().Name}");
            }
        }
        
        if (editDevice is Embedded)
        {
            if (_devices[targetDeviceIndex] is Embedded)
            {
                _devices[targetDeviceIndex] = editDevice;
            }
            else
            {
                throw new ArgumentException($"Type mismatch between devices. " +
                                            $"Target device has type {_devices[targetDeviceIndex].GetType().Name}");
            }
        }
    }

    public void RemoveDeviceById(string deviceId)
    {
        var deviceToRemove = _devices.FirstOrDefault(d => d.Id == deviceId);
        if (deviceToRemove == null) throw new ArgumentException("Device not found.");
        
        _devices.Remove(deviceToRemove);
    }

    public void ShowAllDevices()
    {
        foreach (var device in _devices)
        {
            Console.WriteLine(device.ToString());
        }
    }

    public Device GetDeviceById(string id)
    {
        return _devices.FirstOrDefault(d => d.Id == id);
    }

    public void TurnOnDevice(string id)
    {
        var device = GetDeviceById(id);
        device?.TurnOn();
    }

    public void TurnOffDevice(string id)
    {
        var device = GetDeviceById(id);
        device?.TurnOff();
    }

    
}

/// <summary>
/// Base class for all devices.
/// </summary>
public abstract class Device
{
    public string Id { get; protected set; }
    public string Name { get; set; }
    public bool IsEnabled { get; set; }

    public Device(string id, string name, bool isEnabled)
    {
        Id = id;
        Name = name;
        IsEnabled = isEnabled;
    }

    public virtual void TurnOn()
    {
        IsEnabled = true;
    }

    public virtual void TurnOff()
    {
        IsEnabled = false;
    }

    public abstract void Update(Device updatedDevice);
}

/// <summary>
/// Smartwatch device class.
/// </summary>
public class Smartwatch : Device, IPowerNotify
{
    public int BatteryLevel { get; set; }

    public Smartwatch(string id, string name, bool isEnabled, int batteryLevel) : base(id, name, isEnabled)
    {
        BatteryLevel = batteryLevel;
    }

    public override void Update(Device updatedDevice)
    {
        if (updatedDevice is Smartwatch updatedSmartwatch)
        {
            BatteryLevel = updatedSmartwatch.BatteryLevel;
        }
    }

    public void Notify()
    {
        Console.WriteLine($"Battery level is low. Current level is: {BatteryLevel}");
    }

    public override void TurnOn()
    {
        if (BatteryLevel < 11)
        {
            throw new EmptyBatteryException();
        }

        base.TurnOn();
        BatteryLevel -= 10;

        if (BatteryLevel < 20)
        {
            Notify();
        }
    }

    public override string ToString()
    {
        return $"Smartwatch {Name} ({Id}) is {(IsEnabled ? "enabled" : "disabled")} and has {BatteryLevel}% battery";
    }
}

/// <summary>
/// Embedded device class.
/// </summary>
public class Embedded : Device
{
    public string IpAddress { get; set; }
    public string NetworkName { get; set; }

    public Embedded(string id, string name, bool isEnabled, string ipAddress, string networkName) : base(id, name, isEnabled)
    {
        IpAddress = ipAddress;
        NetworkName = networkName;
    }

    public override void Update(Device updatedDevice)
    {
        if (updatedDevice is Embedded updatedEmbedded)
        {
            IpAddress = updatedEmbedded.IpAddress;
            NetworkName = updatedEmbedded.NetworkName;
        }
    }

    public override void TurnOn()
    {
        base.TurnOn();
        // Additional logic for connecting to network, if needed
    }

    public override string ToString()
    {
        return $"Embedded device {Name} ({Id}) is {(IsEnabled ? "enabled" : "disabled")} and has IP address {IpAddress}";
    }
}

/// <summary>
/// PersonalComputer device class.
/// </summary>
public class PersonalComputer : Device
{
    public string OperatingSystem { get; set; }

    public PersonalComputer(string id, string name, bool isEnabled, string operatingSystem) 
        : base(id, name, isEnabled)
    {
        OperatingSystem = operatingSystem;
    }

    public override void Update(Device updatedDevice)
    {
        if (updatedDevice is PersonalComputer updatedPC)
        {
            OperatingSystem = updatedPC.OperatingSystem;
        }
    }

    public override void TurnOn()
    {
        base.TurnOn();
        Console.WriteLine($"Personal computer {Name} is now on.");
    }

    public override string ToString()
    {
        return $"Personal Computer {Name} ({Id}) is {(IsEnabled ? "enabled" : "disabled")} with OS {OperatingSystem}";
    }
}

/// <summary>
/// Interface for devices that need to notify when power is low.
/// </summary>
public interface IPowerNotify
{
    void Notify();
}

/// <summary>
/// Custom exception for when battery level is too low.
/// </summary>
public class EmptyBatteryException : Exception
{
    public EmptyBatteryException() : base("Battery level is too low to turn it on.") { }
}

/// <summary>
/// Main program entry point.
/// </summary>
public class Program
{
    public static void Main()
    {
        try
        {
            // Creating DeviceManager instance using Factory Pattern
            DeviceManager deviceManager = DeviceManagerFactory.CreateDeviceManager();

            Console.WriteLine("Devices presented after file read.");
            deviceManager.ShowAllDevices();

            Console.WriteLine("Create new personal computer and add it to device store.");
            {
                PersonalComputer computer = new("P-2", "ThinkPad T440", false, null);
                deviceManager.AddDevice(computer);
            }

            Console.WriteLine("Let's try to enable this PC");
            

            Console.WriteLine("Let's install OS for this PC");
            PersonalComputer editComputer = new("P-2", "ThinkPad T440", true, "Arch Linux");
            deviceManager.EditDevice(editComputer);

            Console.WriteLine("Let's try to enable this PC");
            deviceManager.TurnOnDevice("P-2");

            Console.WriteLine("Let's turn off this PC");
            deviceManager.TurnOffDevice("P-2");

            Console.WriteLine("Delete this PC");
            deviceManager.RemoveDeviceById("P-2");

            Console.WriteLine("Devices presented after all operations.");
            deviceManager.ShowAllDevices();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
