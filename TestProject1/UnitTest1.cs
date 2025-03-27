using System;
using Xunit;
using System.IO;
using System.Linq;

public class DeviceManagerTests : IDisposable
{
    private readonly string _testFilePath = "test_devices.txt";
    private readonly DeviceManager _deviceManager;

    public DeviceManagerTests()
    {
        // Create test file with initial devices
        File.WriteAllLines(_testFilePath, new[] {
            "SW-1,Apple Watch SE2,True,27%",
            "P-1,LinuxPC,False,Linux Mint"
        });

        // Create DeviceManager using factory with test file
        _deviceManager = DeviceManagerFactory.CreateDeviceManager(_testFilePath);
    }

    public void Dispose()
    {
        // Clean up test file
        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }
    }

    [Fact]
    public void AddDevice_Should_Add_New_Device()
    {
        // Arrange
        var newDevice = new PersonalComputer("P-2", "ThinkPad T440", false, "Ubuntu");

        // Act
        _deviceManager.AddDevice(newDevice);
        var retrievedDevice = _deviceManager.GetDeviceById("P-2");

        // Assert
        Assert.NotNull(retrievedDevice);
        Assert.Equal("ThinkPad T440", retrievedDevice.Name);
        Assert.IsType<PersonalComputer>(retrievedDevice);
    }

    [Fact]
    public void AddDevice_Should_Throw_Exception_If_Duplicate_ID()
    {
        // Arrange
        var duplicateDevice = new Smartwatch("SW-1", "Apple Watch SE2", true, 27);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _deviceManager.AddDevice(duplicateDevice));
        Assert.Contains("Device with ID SW-1 already exists", exception.Message);
    }

    [Fact]
    public void AddDevice_Should_Throw_Exception_When_Capacity_Exceeded()
    {
        // Arrange - fill up the repository
        for (int i = 2; i <= 15; i++)
        {
            _deviceManager.AddDevice(new PersonalComputer($"P-{i}", $"PC{i}", false, "OS"));
        }

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            _deviceManager.AddDevice(new PersonalComputer("P-16", "ExtraPC", false, "OS")));
        Assert.Contains("Device storage is full", exception.Message);
    }

    [Fact]
    public void RemoveDeviceById_Should_Remove_Existing_Device()
    {
        // Act
        _deviceManager.RemoveDeviceById("SW-1");
        var device = _deviceManager.GetDeviceById("SW-1");

        // Assert
        Assert.Null(device);
    }

    [Fact]
    public void RemoveDeviceById_Should_Throw_Exception_If_Device_Not_Exists()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _deviceManager.RemoveDeviceById("P-99"));
        Assert.Contains("Device with ID P-99 not found", exception.Message);
    }

    [Fact]
    public void TurnOnDevice_Should_Enable_Device()
    {
        // Act
        _deviceManager.TurnOnDevice("P-1");
        var device = _deviceManager.GetDeviceById("P-1");

        // Assert
        Assert.True(device?.IsEnabled);
    }

    [Fact]
    public void TurnOnDevice_Should_Throw_For_Nonexistent_Device()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _deviceManager.TurnOnDevice("P-99"));
        Assert.Contains("Device with ID P-99 is not stored", exception.Message);
    }

    [Fact]
    public void TurnOffDevice_Should_Disable_Device()
    {
        // Arrange - ensure device is on first
        _deviceManager.TurnOnDevice("SW-1");

        // Act
        _deviceManager.TurnOffDevice("SW-1");
        var device = _deviceManager.GetDeviceById("SW-1");

        // Assert
        Assert.False(device?.IsEnabled);
    }

    [Fact]
    public void EditDevice_Should_Update_Existing_Device()
    {
        // Arrange
        var updatedDevice = new PersonalComputer("P-1", "UpdatedPC", true, "NewOS");

        // Act
        _deviceManager.EditDevice(updatedDevice);
        var device = _deviceManager.GetDeviceById("P-1") as PersonalComputer;

        // Assert
        Assert.NotNull(device);
        Assert.Equal("UpdatedPC", device.Name);
        Assert.Equal("NewOS", device.OperatingSystem);
        Assert.True(device.IsEnabled);
    }

    [Fact]
    public void EditDevice_Should_Throw_Exception_If_Device_Not_Found()
    {
        // Arrange
        var unknownDevice = new PersonalComputer("P-99", "Unknown PC", true, "Windows");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _deviceManager.EditDevice(unknownDevice));
        Assert.Contains("Device with ID P-99 not found", exception.Message);
    }

    [Fact]
    public void EditDevice_Should_Throw_For_Type_Mismatch()
    {
        // Arrange - trying to replace a Smartwatch with a PC
        var wrongTypeDevice = new PersonalComputer("SW-1", "WrongType", true, "OS");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _deviceManager.EditDevice(wrongTypeDevice));
        Assert.Contains("Type mismatch between devices", exception.Message);
    }

    [Fact]
    public void GetDeviceById_Should_Return_Null_For_Nonexistent_Device()
    {
        // Act
        var device = _deviceManager.GetDeviceById("P-99");

        // Assert
        Assert.Null(device);
    }

    [Fact]
    public void ShowAllDevices_Should_Display_All_Devices()
    {
        // Arrange
        var consoleOutput = new StringWriter();
        Console.SetOut(consoleOutput);

        // Act
        _deviceManager.ShowAllDevices();
        string output = consoleOutput.ToString();

        // Assert
        Assert.Contains("Apple Watch SE2", output);
        Assert.Contains("LinuxPC", output);

        // Clean up
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()));
    }

    [Fact]
    public void SaveDevices_Should_Save_All_Devices_To_File()
    {
        // Arrange
        var outputPath = "test_output.txt";
        if (File.Exists(outputPath))
        {
            File.Delete(outputPath);
        }

        // Act
        _deviceManager.SaveDevices(outputPath);
        var lines = File.ReadAllLines(outputPath);

        // Assert
        Assert.Equal(2, lines.Length);
        Assert.Contains("SW-1,Apple Watch SE2,True,27%", lines[0]);
        Assert.Contains("P-1,LinuxPC,False,Linux Mint", lines[1]);

        // Clean up
        File.Delete(outputPath);
    }
}