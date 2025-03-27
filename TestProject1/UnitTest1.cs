using System;
using System.Collections.Generic;
using Xunit;

public class DeviceManagerTests
{
    private readonly DeviceManager _deviceManager;

    public DeviceManagerTests()
    {
        _deviceManager = new DeviceManager("/Users/deryaogus/Desktop/APBD-3/TestProject1/test_input.txt");
        _deviceManager.AddDevice(new Smartwatch("SW-1", "Apple Watch SE2", true, 27));
        _deviceManager.AddDevice(new PersonalComputer("P-1", "LinuxPC", false, "Linux Mint"));
    }

    [Fact]
    public void AddDevice_Should_Add_New_Device()
    {
        var newDevice = new PersonalComputer("P-2", "ThinkPad T440", false, "Ubuntu");
        _deviceManager.AddDevice(newDevice);
        var retrievedDevice = _deviceManager.GetDeviceById("P-2");
        Assert.NotNull(retrievedDevice);
        Assert.Equal("ThinkPad T440", retrievedDevice.Name);
    }

    [Fact]
    public void AddDevice_Should_Throw_Exception_If_Duplicate_ID()
    {
        var duplicateDevice = new Smartwatch("SW-1", "Apple Watch SE2", true, 27);
        var exception = Assert.Throws<ArgumentException>(() => _deviceManager.AddDevice(duplicateDevice));
        Assert.Contains("Device with ID SW-1 is already stored.", exception.Message);
    }

    

    [Fact]
    public void RemoveDevice_Should_Throw_Exception_If_Device_Not_Exists()
    {
        var exception = Assert.Throws<ArgumentException>(() => _deviceManager.RemoveDeviceById("P-99"));
        Assert.Contains("Device with ID P-99 is not stored.", exception.Message);
    }

    [Fact]
    public void TurnOnDevice_Should_Enable_Device()
    {
        _deviceManager.TurnOnDevice("P-1");
        var device = _deviceManager.GetDeviceById("P-1");
        Assert.True(device.IsEnabled);
    }

    [Fact]
    public void TurnOnDevice_Should_Throw_Exception_If_Device_Not_Found()
    {
        var exception = Assert.Throws<ArgumentException>(() => _deviceManager.TurnOnDevice("P-99"));
        Assert.Contains("Device with ID P-99 is not stored.", exception.Message);
    }

    

    [Fact]
    public void EditDevice_Should_Throw_Exception_If_Device_Not_Found()
    {
        var unknownDevice = new PersonalComputer("P-99", "Unknown PC", true, "Windows");
        var exception = Assert.Throws<ArgumentException>(() => _deviceManager.EditDevice(unknownDevice));
        Assert.Contains("Device with ID P-99 is not stored.", exception.Message);
    }
}
