using System.Text;
using System.Text.RegularExpressions;


/// <summary>
/// Factory for creating DeviceManager instances
/// </summary>
public static class DeviceManagerFactory
{
    /// <summary>
    /// Creates a new DeviceManager instance with devices loaded from the specified file
    /// </summary>
    /// <param name="filePath">Path to the input device file</param>
    /// <returns>New DeviceManager instance</returns>
    public static DeviceManager CreateDeviceManager(string filePath)
    {
        return new DeviceManager(filePath, new DeviceParser(), new DeviceRepository());
    }
}

/// <summary>
/// Main class for managing devices
/// </summary>
public class DeviceManager
{
    private readonly string _inputDeviceFile;
    private readonly DeviceParser _deviceParser;
    private readonly DeviceRepository _deviceRepository;

    /// <summary>
    /// Initializes a new instance of DeviceManager
    /// </summary>
    /// <param name="filePath">Path to input file</param>
    /// <param name="deviceParser">Device parser instance</param>
    /// <param name="deviceRepository">Device repository instance</param>
    public DeviceManager(string filePath, DeviceParser deviceParser, DeviceRepository deviceRepository)
    {
        _inputDeviceFile = filePath;
        _deviceParser = deviceParser;
        _deviceRepository = deviceRepository;

        if (!File.Exists(_inputDeviceFile))
        {
            throw new FileNotFoundException("The input device file could not be found.");
        }

        var lines = File.ReadAllLines(_inputDeviceFile);
        ParseDevices(lines);
    }

    /// <summary>
    /// Adds a new device to the repository
    /// </summary>
    /// <param name="newDevice">Device to add</param>
    public void AddDevice(Device newDevice) => _deviceRepository.Add(newDevice);

    /// <summary>
    /// Updates an existing device
    /// </summary>
    /// <param name="editDevice">Device with updated properties</param>
    public void EditDevice(Device editDevice) => _deviceRepository.Update(editDevice);

    /// <summary>
    /// Removes a device by its ID
    /// </summary>
    /// <param name="deviceId">ID of device to remove</param>
    public void RemoveDeviceById(string deviceId) => _deviceRepository.Remove(deviceId);

    /// <summary>
    /// Turns on a device by its ID
    /// </summary>
    /// <param name="id">Device ID</param>
    public void TurnOnDevice(string id)
    {
        var device = _deviceRepository.GetById(id) ?? throw new ArgumentException($"Device with ID {id} is not stored.", nameof(id));
        device.TurnOn();
    }

    /// <summary>
    /// Turns off a device by its ID
    /// </summary>
    /// <param name="id">Device ID</param>
    public void TurnOffDevice(string id)
    {
        var device = _deviceRepository.GetById(id) ?? throw new ArgumentException($"Device with ID {id} is not stored.", nameof(id));
        device.TurnOff();
    }

    /// <summary>
    /// Gets a device by its ID
    /// </summary>
    /// <param name="id">Device ID</param>
    /// <returns>The device or null if not found</returns>
    public Device? GetDeviceById(string id) => _deviceRepository.GetById(id);

    /// <summary>
    /// Displays all devices in the repository
    /// </summary>
    public void ShowAllDevices()
    {
        foreach (var device in _deviceRepository.GetAll())
        {
            Console.WriteLine(device.ToString());
        }
    }

    /// <summary>
    /// Saves all devices to the specified file
    /// </summary>
    /// <param name="outputPath">Path to output file</param>
    public void SaveDevices(string outputPath)
    {
        var devicesSb = new StringBuilder();
        foreach (var device in _deviceRepository.GetAll())
        {
            devicesSb.AppendLine(device.ToCsvString());
        }
        File.WriteAllText(outputPath, devicesSb.ToString());
    }

    private void ParseDevices(string[] lines)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            try
            {
                Device parsedDevice = lines[i] switch
                {
                    string s when s.StartsWith("P-") => _deviceParser.ParsePC(lines[i], i),
                    string s when s.StartsWith("SW-") => _deviceParser.ParseSmartwatch(lines[i], i),
                    string s when s.StartsWith("ED-") => _deviceParser.ParseEmbedded(lines[i], i),
                    _ => throw new ArgumentException($"Line {i} is corrupted.")
                };
                AddDevice(parsedDevice);
            }
            catch (ArgumentException argEx)
            {
                Console.WriteLine(argEx.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing line {i}: {lines[i]}. Error: {ex.Message}");
            }
        }
    }
}

/// <summary>
/// Handles device storage operations
/// </summary>
public class DeviceRepository
{
    private const int MaxCapacity = 15;
    private readonly List<Device> _devices = new(MaxCapacity);

    /// <summary>
    /// Gets all devices in the repository
    /// </summary>
    public IEnumerable<Device> GetAll() => _devices.AsReadOnly();

    /// <summary>
    /// Adds a new device to the repository
    /// </summary>
    /// <param name="device">Device to add</param>
    public void Add(Device device)
    {
        if (_devices.Any(d => d.Id == device.Id))
        {
            throw new ArgumentException($"Device with ID {device.Id} already exists.");
        }

        if (_devices.Count >= MaxCapacity)
        {
            throw new InvalidOperationException("Device storage is full.");
        }

        _devices.Add(device);
    }

    /// <summary>
    /// Updates an existing device
    /// </summary>
    /// <param name="device">Device with updated properties</param>
    public void Update(Device device)
    {
        var existingDevice = GetById(device.Id) ?? throw new ArgumentException($"Device with ID {device.Id} not found.");
        
        if (existingDevice.GetType() != device.GetType())
        {
            throw new ArgumentException($"Type mismatch between devices. Target is {existingDevice.GetType().Name}");
        }

        _devices.Remove(existingDevice);
        _devices.Add(device);
    }

    /// <summary>
    /// Removes a device by its ID
    /// </summary>
    /// <param name="deviceId">ID of device to remove</param>
    public void Remove(string deviceId)
    {
        var device = GetById(deviceId) ?? throw new ArgumentException($"Device with ID {deviceId} not found.");
        _devices.Remove(device);
    }

    /// <summary>
    /// Gets a device by its ID
    /// </summary>
    /// <param name="id">Device ID</param>
    /// <returns>The device or null if not found</returns>
    public Device? GetById(string id) => _devices.FirstOrDefault(d => d.Id == id);
}

/// <summary>
/// Base class for all devices
/// </summary>
public abstract class Device
{
    /// <summary>
    /// Gets or sets the device ID
    /// </summary>
    public string Id { get; protected set; }

    /// <summary>
    /// Gets or sets the device name
    /// </summary>
    public string Name { get; protected set; }

    /// <summary>
    /// Gets or sets whether the device is enabled
    /// </summary>
    public bool IsEnabled { get; protected set; }

    /// <summary>
    /// Initializes a new instance of Device
    /// </summary>
    /// <param name="id">Device ID</param>
    /// <param name="name">Device name</param>
    /// <param name="isEnabled">Enabled status</param>
    protected Device(string id, string name, bool isEnabled)
    {
        Id = id;
        Name = name;
        IsEnabled = isEnabled;
    }

    /// <summary>
    /// Turns on the device
    /// </summary>
    public virtual void TurnOn() => IsEnabled = true;

    /// <summary>
    /// Turns off the device
    /// </summary>
    public virtual void TurnOff() => IsEnabled = false;

    /// <summary>
    /// Converts device to CSV string representation
    /// </summary>
    /// <returns>CSV string</returns>
    public abstract string ToCsvString();
}

/// <summary>
/// Represents a personal computer device
/// </summary>
public class PersonalComputer : Device
{
    /// <summary>
    /// Gets or sets the operating system
    /// </summary>
    public string? OperatingSystem { get; set; }

    /// <summary>
    /// Initializes a new instance of PersonalComputer
    /// </summary>
    public PersonalComputer(string id, string name, bool isEnabled, string? operatingSystem) 
        : base(id, name, isEnabled)
    {
        if (!id.StartsWith("P-"))
        {
            throw new ArgumentException("Invalid ID format. Must start with 'P-'");
        }
        OperatingSystem = operatingSystem;
    }

    /// <summary>
    /// Turns on the computer
    /// </summary>
    public override void TurnOn()
    {
        if (OperatingSystem is null)
        {
            throw new EmptySystemException();
        }
        base.TurnOn();
    }

    /// <summary>
    /// Returns string representation of the computer
    /// </summary>
    public override string ToString()
    {
        string enabledStatus = IsEnabled ? "enabled" : "disabled";
        string osStatus = OperatingSystem is null ? "has not OS" : $"has {OperatingSystem}";
        return $"PC {Name} ({Id}) is {enabledStatus} and {osStatus}";
    }

    /// <summary>
    /// Converts to CSV string
    /// </summary>
    public override string ToCsvString()
    {
        return $"{Id},{Name},{IsEnabled},{OperatingSystem}";
    }
}

/// <summary>
/// Represents a smartwatch device
/// </summary>
public class Smartwatch : Device, IPowerNotify
{
    private int _batteryLevel;

    /// <summary>
    /// Gets or sets the battery level (0-100)
    /// </summary>
    public int BatteryLevel
    {
        get => _batteryLevel;
        set
        {
            if (value < 0 || value > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Battery level must be 0-100");
            }
            _batteryLevel = value;
            if (_batteryLevel < 20)
            {
                Notify();
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of Smartwatch
    /// </summary>
    public Smartwatch(string id, string name, bool isEnabled, int batteryLevel) 
        : base(id, name, isEnabled)
    {
        if (!id.StartsWith("SW-"))
        {
            throw new ArgumentException("Invalid ID format. Must start with 'SW-'");
        }
        BatteryLevel = batteryLevel;
    }

    /// <summary>
    /// Notifies about low battery
    /// </summary>
    public void Notify()
    {
        Console.WriteLine($"Battery level is low. Current level: {BatteryLevel}%");
    }

    /// <summary>
    /// Turns on the smartwatch
    /// </summary>
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

    /// <summary>
    /// Returns string representation of the smartwatch
    /// </summary>
    public override string ToString()
    {
        string enabledStatus = IsEnabled ? "enabled" : "disabled";
        return $"Smartwatch {Name} ({Id}) is {enabledStatus} and has {BatteryLevel}%";
    }

    /// <summary>
    /// Converts to CSV string
    /// </summary>
    public override string ToCsvString()
    {
        return $"{Id},{Name},{IsEnabled},{BatteryLevel}%";
    }
}

/// <summary>
/// Represents an embedded device
/// </summary>
public class Embedded : Device
{
    private string _ipAddress;
    private bool _isConnected = false;

    /// <summary>
    /// Gets or sets the network name
    /// </summary>
    public string NetworkName { get; set; }

    /// <summary>
    /// Gets or sets the IP address
    /// </summary>
    public string IpAddress
    {
        get => _ipAddress;
        set
        {
            var ipRegex = new Regex("^((25[0-5]|(2[0-4]|1\\d|[1-9]|)\\d)\\.?\\b){4}$");
            if (ipRegex.IsMatch(value))
            {
                _ipAddress = value;
            }
            else
            {
                throw new ArgumentException("Invalid IP address format.");
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of Embedded device
    /// </summary>
    public Embedded(string id, string name, bool isEnabled, string ipAddress, string networkName) 
        : base(id, name, isEnabled)
    {
        if (!id.StartsWith("ED-"))
        {
            throw new ArgumentException("Invalid ID format. Must start with 'ED-'");
        }
        IpAddress = ipAddress;
        NetworkName = networkName;
    }

    /// <summary>
    /// Turns on the embedded device
    /// </summary>
    public override void TurnOn()
    {
        Connect();
        base.TurnOn();
    }

    /// <summary>
    /// Turns off the embedded device
    /// </summary>
    public override void TurnOff()
    {
        _isConnected = false;
        base.TurnOff();
    }

    /// <summary>
    /// Returns string representation of the embedded device
    /// </summary>
    public override string ToString()
    {
        string enabledStatus = IsEnabled ? "enabled" : "disabled";
        return $"Embedded device {Name} ({Id}) is {enabledStatus} and has IP {IpAddress}";
    }

    /// <summary>
    /// Converts to CSV string
    /// </summary>
    public override string ToCsvString()
    {
        return $"{Id},{Name},{IsEnabled},{IpAddress},{NetworkName}";
    }

    private void Connect()
    {
        if (!NetworkName.Contains("MD Ltd."))
        {
            throw new ConnectionException();
        }
        _isConnected = true;
    }
}

/// <summary>
/// Parses device data from strings
/// </summary>
public class DeviceParser
{
    private const int MinimumRequiredElements = 4;
    private const int IndexPosition = 0;
    private const int DeviceNamePosition = 1;
    private const int EnabledStatusPosition = 2;

    /// <summary>
    /// Parses a personal computer from string data
    /// </summary>
    public PersonalComputer ParsePC(string line, int lineNumber)
    {
        const int SystemPosition = 3;
        var infoSplits = line.Split(',');

        ValidateLine(infoSplits, lineNumber, line);

        if (!bool.TryParse(infoSplits[EnabledStatusPosition], out _))
        {
            throw new ArgumentException($"Corrupted line {lineNumber}: can't parse enabled status for computer.");
        }

        return new PersonalComputer(
            infoSplits[IndexPosition],
            infoSplits[DeviceNamePosition],
            bool.Parse(infoSplits[EnabledStatusPosition]),
            infoSplits.Length > SystemPosition ? infoSplits[SystemPosition] : null);
    }

    /// <summary>
    /// Parses a smartwatch from string data
    /// </summary>
    public Smartwatch ParseSmartwatch(string line, int lineNumber)
    {
        const int BatteryPosition = 3;
        var infoSplits = line.Split(',');

        ValidateLine(infoSplits, lineNumber, line);

        if (!bool.TryParse(infoSplits[EnabledStatusPosition], out _))
        {
            throw new ArgumentException($"Corrupted line {lineNumber}: can't parse enabled status for smartwatch.");
        }

        if (!int.TryParse(infoSplits[BatteryPosition].Replace("%", ""), out int batteryLevel))
        {
            throw new ArgumentException($"Corrupted line {lineNumber}: can't parse battery level for smartwatch.");
        }

        return new Smartwatch(
            infoSplits[IndexPosition],
            infoSplits[DeviceNamePosition],
            bool.Parse(infoSplits[EnabledStatusPosition]),
            batteryLevel);
    }

    /// <summary>
    /// Parses an embedded device from string data
    /// </summary>
    public Embedded ParseEmbedded(string line, int lineNumber)
    {
        const int IpAddressPosition = 3;
        const int NetworkNamePosition = 4;
        var infoSplits = line.Split(',');

        if (infoSplits.Length < MinimumRequiredElements + 1)
        {
            throw new ArgumentException($"Corrupted line {lineNumber}");
        }

        if (!bool.TryParse(infoSplits[EnabledStatusPosition], out _))
        {
            throw new ArgumentException($"Corrupted line {lineNumber}: can't parse enabled status for embedded device.");
        }

        return new Embedded(
            infoSplits[IndexPosition],
            infoSplits[DeviceNamePosition],
            bool.Parse(infoSplits[EnabledStatusPosition]),
            infoSplits[IpAddressPosition],
            infoSplits[NetworkNamePosition]);
    }

    private void ValidateLine(string[] infoSplits, int lineNumber, string line)
    {
        if (infoSplits.Length < MinimumRequiredElements)
        {
            throw new ArgumentException($"Corrupted line {lineNumber}", line);
        }
    }
}

/// <summary>
/// Interface for power notifications
/// </summary>
public interface IPowerNotify
{
    /// <summary>
    /// Notifies about power status
    /// </summary>
    void Notify();
}

/// <summary>
/// Exception for missing operating system
/// </summary>
public class EmptySystemException : Exception
{
    public EmptySystemException() : base("Operating system is not installed.") { }
}

/// <summary>
/// Exception for low battery
/// </summary>
public class EmptyBatteryException : Exception
{
    public EmptyBatteryException() : base("Battery level is too low to turn on.") { }
}

/// <summary>
/// Exception for connection issues
/// </summary>
public class ConnectionException : Exception
{
    public ConnectionException() : base("Wrong network name.") { }
}

class Program
{
    public static void Main()
    {
        try
        {
            // Changed to use factory pattern
            DeviceManager deviceManager = DeviceManagerFactory.CreateDeviceManager("/Users/deryaogus/Desktop/APBD-3/APBD-3/input.txt");
            
            Console.WriteLine("Devices presented after file read.");
            deviceManager.ShowAllDevices();
            
            Console.WriteLine("Create new computer with correct data and add it to device store.");
            {
                PersonalComputer computer = new("P-2", "ThinkPad T440", false, null);
                deviceManager.AddDevice(computer);
            }
            
            Console.WriteLine("Let's try to enable this PC");
            try
            {
                deviceManager.TurnOnDevice("P-2");
            }
            catch (EmptySystemException ex)
            {
                Console.WriteLine(ex.Message);
            }
            
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