﻿// See https://aka.ms/new-console-template for more information
using DeviceTest;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Configuration;
using MPPTSunDevice;
using System.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;

string deviceName = "/dev/ttyUSB0";
//string deviceName = "COM1";
using var device=SolarChargeController.OpenDevice(deviceName);
Console.WriteLine(device.ReadLoad());
Console.WriteLine(device.ReadBatteryVoltage());
Console.WriteLine(device.ReadPanel());
Console.WriteLine($"SOC={device.ReadBatterySOC():##.0%}");
Console.WriteLine($"Workload type:{device.ReadWorkloadType()}");
Console.Write($"DC output:");
Console.WriteLine(device.IsDCOutput ? "On" : "Off");

var config = new ConfigurationBuilder().AddXmlFile("App.config").Build();
string? cnn = config["IothubConnectionString"];
if (cnn==null)
{
    return;
}
DeviceClient iotDevice = DeviceClient.CreateFromConnectionString(cnn);
await iotDevice.SetMethodHandlerAsync("SetOutput", SetOutput, null);
//device.SetManualOutput(false);
//Console.WriteLine("Manual output set complete");
//return;


while (true)
{
    try
    {
        DeviceTelemetryMessage content = new DeviceTelemetryMessage();
        var panel = device.ReadPanel();
        content.SolarPanel_V = panel.v;
        content.SolarPanel_W = panel.w;
        content.SolarPanel_A = panel.a;

        var l = device.ReadLoad();
        content.Load_A = l.a;
        content.Load_V = l.v;
        content.Load_W = l.w;

        content.Battery_V = device.ReadBatteryVoltage();
        MemoryStream ms = new MemoryStream();
        JsonSerializer.Serialize<DeviceTelemetryMessage>(ms, content);
        ms.Position = 0;
        Message msg = new Message(ms);
        await iotDevice.SendEventAsync(msg);

        Console.WriteLine($"{DateTime.Now} Device Message Sent");
    }
    catch (Exception ex)
    {

        Console.WriteLine(ex);
    }
    
    await Task.Delay(10000);
}

Task<MethodResponse> SetOutput(MethodRequest methodRequest, object userContext)
{
    var para = System.Text.Json.JsonSerializer.Deserialize<SetOutputMethodParameters>(methodRequest.DataAsJson);
    device.SetManualOutput(para.IsOn);
    Console.WriteLine($"SetOutput={para.IsOn}");
    return Task.FromResult(new MethodResponse(200));
}

