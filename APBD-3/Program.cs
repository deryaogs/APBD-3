using System.Text;
using System.Text.RegularExpressions;

namespace Tutorial3_Task;

class DeviceManager
{
    private readonly DeviceParser _deviceParser = new DeviceParser();
    private string _inputDeviceFile;
    private const int MaxCapacity = 15;
    private List<Device> _devices = new(capacity: MaxCapacity);

    public DeviceManager(string filePath)
    {
        _inputDeviceFile = filePath;

        if (!File.Exists(_inputDeviceFile))
        {
            throw new FileNotFoundException("The input device file could not be found.");
        }

        var lines = File.ReadAllLines(_inputDeviceFile);
        ParseDevices(lines);
    }

    public void AddDevice(Device newDevice)
    {
        foreach (var storedDevice in _devices)
        {
            if (storedDevice.Id.Equals(newDevice.Id))
            {
                throw new ArgumentException($"Device with ID {storedDevice.Id} is already stored.", nameof(newDevice));
            }
        }

        if (_devices.Count >= MaxCapacity)
        {
            throw new Exception("Device storage is full.");
        }
        
        _devices.Add(newDevice);
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
        Device? targetDevice = null;
        foreach (var storedDevice in _devices)
        {
            if (storedDevice.Id.Equals(deviceId))
            {
                targetDevice = storedDevice;
                break;
            }
        }

        if (targetDevice == null)
        {
            throw new ArgumentException($"Device with ID {deviceId} is not stored.", nameof(deviceId));
        }
        
        _devices.Remove(targetDevice);
    }

    public void TurnOnDevice(string id)
    {
        foreach (var storedDevice in _devices)
        {
            if (storedDevice.Id.Equals(id))
            {
                storedDevice.TurnOn();
                return;
            }
        }
        
        throw new ArgumentException($"Device with ID {id} is not stored.", nameof(id));
    }

    public void TurnOffDevice(string id)
    {
        foreach (var storedDevice in _devices)
        {
            if (storedDevice.Id.Equals(id))
            {
                storedDevice.TurnOff();
                return;
            }
        }
        
        throw new ArgumentException($"Device with ID {id} is not stored.", nameof(id));
    }

    public Device? GetDeviceById(string id)
    {
        foreach (var storedDevice in _devices)
        {
            if (storedDevice.Id.Equals(id))
            {
                return storedDevice;
            }
        }

        return null;
    }

    public void ShowAllDevices()
    {
        foreach (var storedDevices in _devices)
        {
            Console.WriteLine(storedDevices.ToString());
        }
    }

    public void SaveDevices(string outputPath)
    {
        StringBuilder devicesSb = new();

        foreach (var storedDevice in _devices)
        {
            if (storedDevice is Smartwatch smartwatchCopy)
            {
                devicesSb.AppendLine($"{smartwatchCopy.Id},{smartwatchCopy.Name}," +
                                     $"{smartwatchCopy.IsEnabled},{smartwatchCopy.BatteryLevel}%");
            }
            else if (storedDevice is PersonalComputer pcCopy)
            {
                devicesSb.AppendLine($"{pcCopy.Id},{pcCopy.Name}," +
                                     $"{pcCopy.IsEnabled},{pcCopy.OperatingSystem}");
            }
            else
            {
                var embeddedCopy = storedDevice as Embedded;
                devicesSb.AppendLine($"{embeddedCopy.Id},{embeddedCopy.Name}," +
                                     $"{embeddedCopy.IsEnabled},{embeddedCopy.IpAddress}," +
                                     $"{embeddedCopy.NetworkName}");
            }
        }
        
        File.WriteAllLines(outputPath, devicesSb.ToString().Split('\n'));
    }

    private void ParseDevices(string[] lines)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            try
            {
                Device parsedDevice;
                    
                if (lines[i].StartsWith("P-"))
                {
                    parsedDevice = _deviceParser.ParsePC(lines[i], i);
                }
                else if (lines[i].StartsWith("SW-"))
                {
                    parsedDevice = _deviceParser.ParseSmartwatch(lines[i], i);
                }
                else if (lines[i].StartsWith("ED-"))
                {
                    parsedDevice = _deviceParser.ParseEmbedded(lines[i], i);
                }
                else
                {
                    throw new ArgumentException($"Line {i} is corrupted.");
                }
                    
                AddDevice(parsedDevice);
            }
            catch (ArgumentException argEx)
            {
                Console.WriteLine(argEx.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Something went wrong during parsing this line: {lines[i]}. The exception message: {ex.Message}");
            }
        }
    }
}

abstract class Device
{
    public string Id { get; set; }
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
}

class PersonalComputer : Device
{
    public string? OperatingSystem { get; set; }
    
    public PersonalComputer(string id, string name, bool isEnabled, string? operatingSystem) : base(id, name, isEnabled)
    {
        if (!CheckId(id))
        {
            throw new ArgumentException("Invalid ID value. Required format: P-1", id);
        }
        
        OperatingSystem = operatingSystem;
    }

    public override void TurnOn()
    {
        if (OperatingSystem is null)
        {
            throw new EmptySystemException();
        }

        base.TurnOn();
    }

    public override string ToString()
    {
        string enabledStatus = IsEnabled ? "enabled" : "disabled";
        string osStatus = OperatingSystem is null ? "has not OS" : $"has {OperatingSystem}";
        return $"PC {Name} ({Id}) is {enabledStatus} and {osStatus}";
    }

    private bool CheckId(string id) => id.Contains("P-");
}

class Smartwatch : Device, IPowerNotify
{
    private int _batteryLevel;

    public int BatteryLevel
    {
        get => _batteryLevel;
        set
        {
            if (value < 0 || value > 100)
            {
                throw new ArgumentException("Invalid battery level value. Must be between 0 and 100.", nameof(value));
            }
            
            _batteryLevel = value;
            if (_batteryLevel < 20)
            {
                Notify();
            }
        }
    }
    
    public Smartwatch(string id, string name, bool isEnabled, int batteryLevel) : base(id, name, isEnabled)
    {
        if (CheckId(id))
        {
            throw new ArgumentException("Invalid ID value. Required format: SW-1", id);
        }
        BatteryLevel = batteryLevel;
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
        string enabledStatus = IsEnabled ? "enabled" : "disabled";
        return $"Smartwatch {Name} ({Id}) is {enabledStatus} and has {BatteryLevel}%";
    }
    
    private bool CheckId(string id) => id.Contains("E-");
}

class Embedded : Device
{
    public string NetworkName { get; set; }
    private string _ipAddress;
    private bool _isConnected = false;

    public string IpAddress
    {
        get => _ipAddress;
        set
        {
            Regex ipRegex = new Regex("^((25[0-5]|(2[0-4]|1\\d|[1-9]|)\\d)\\.?\\b){4}$");
            if (ipRegex.IsMatch(value))
            {
                _ipAddress = value;
            }

            throw new ArgumentException("Wrong IP address format.");
        }
    }
    
    public Embedded(string id, string name, bool isEnabled, string ipAddress, string networkName) : base(id, name, isEnabled)
    {
        if (CheckId(id))
        {
            throw new ArgumentException("Invalid ID value. Required format: E-1", id);
        }

        IpAddress = ipAddress;
        NetworkName = networkName;
    }

    public override void TurnOn()
    {
        Connect();
        base.TurnOn();
    }

    public override void TurnOff()
    {
        _isConnected = false;
        base.TurnOff();
    }

    public override string ToString()
    {
        string enabledStatus = IsEnabled ? "enabled" : "disabled";
        return $"Embedded device {Name} ({Id}) is {enabledStatus} and has IP address {IpAddress}";
    }

    private void Connect()
    {
        if (NetworkName.Contains("MD Ltd."))
        {
            _isConnected = true;
        }
        else
        {
            throw new ConnectionException();
        }
    }
    
    private bool CheckId(string id) => id.Contains("E-");
}


class EmptySystemException : Exception
{
    public EmptySystemException() : base("Operation system is not installed.") { }
}

class EmptyBatteryException : Exception
{
    public EmptyBatteryException() : base("Battery level is too low to turn it on.") { }
}

class ConnectionException : Exception
{
    public ConnectionException() : base("Wrong netowrk name.") { }
}

class DeviceParser
{
    // Because we should have basic info + at least one additional info
    private const int MinimumRequiredElements = 4;

    private const int IndexPosition = 0;
    private const int DeviceNamePosition = 1;
    private const int EnabledStatusPosition = 2;
    
    public PersonalComputer ParsePC(string line, int lineNumber)
    {
        const int SystemPosition = 3;
        
        var infoSplits = line.Split(',');

        if (infoSplits.Length < MinimumRequiredElements)
        {
            throw new ArgumentException($"Corrupted line {lineNumber}", line);
        }
        
        if (bool.TryParse(infoSplits[EnabledStatusPosition], out bool _) is false)
        {
            throw new ArgumentException($"Corrupted line {lineNumber}: can't parse enabled status for computer.", line);
        }
        
        return new PersonalComputer(infoSplits[IndexPosition], infoSplits[DeviceNamePosition], 
            bool.Parse(infoSplits[EnabledStatusPosition]), infoSplits[SystemPosition]);
    }

    public Smartwatch ParseSmartwatch(string line, int lineNumber)
    {
        const int BatteryPosition = 3;
        
        var infoSplits = line.Split(',');

        if (infoSplits.Length < MinimumRequiredElements)
        {
            throw new ArgumentException($"Corrupted line {lineNumber}", line);
        }
        
        if (bool.TryParse(infoSplits[EnabledStatusPosition], out bool _) is false)
        {
            throw new ArgumentException($"Corrupted line {lineNumber}: can't parse enabled status for smartwatch.", line);
        }

        if (int.TryParse(infoSplits[BatteryPosition].Replace("%", ""), out int _) is false)
        {
            throw new ArgumentException($"Corrupted line {lineNumber}: can't parse battery level for smartwatch.", line);
        }

        return new Smartwatch(infoSplits[IndexPosition], infoSplits[DeviceNamePosition], 
            bool.Parse(infoSplits[EnabledStatusPosition]), int.Parse(infoSplits[BatteryPosition].Replace("%", "")));
    }

    public Embedded ParseEmbedded(string line, int lineNumber)
    {
        const int IpAddressPosition = 3;
        const int NetworkNamePosition = 4;
        
        var infoSplits = line.Split(',');

        if (infoSplits.Length < MinimumRequiredElements + 1)
        {
            throw new ArgumentException($"Corrupted line {lineNumber}", line);
        }
        
        if (bool.TryParse(infoSplits[EnabledStatusPosition], out bool _) is false)
        {
            throw new ArgumentException($"Corrupted line {lineNumber}: can't parse enabled status for embedded device.", line);
        }

        return new Embedded(infoSplits[IndexPosition], infoSplits[DeviceNamePosition], 
            bool.Parse(infoSplits[EnabledStatusPosition]), infoSplits[IpAddressPosition], 
            infoSplits[NetworkNamePosition]);
    }
    
}

interface IPowerNotify
{
    void Notify();
}

class Program
{
    public static void Main()
    {
        try
        {
            DeviceManager deviceManager = new("/Users/deryaogus/Desktop/APBD-3/APBD-3/input.txt");
            
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