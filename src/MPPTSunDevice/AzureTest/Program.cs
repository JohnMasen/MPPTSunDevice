using nanoFramework.Azure.Devices.Client;
using nanoFramework.Hardware.Esp32;
using nanoFramework.Json;
using nanoFramework.Runtime.Native;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading;

namespace AzureTest
{
    public class Program
    {
        public static void Main()
        {
            WaitIP();
            Config cfg;
            using (var fs = File.OpenRead("I:\\iotconfig.json"))
            {
                cfg = (Config)JsonConvert.DeserializeObject(fs, typeof(Config));
            }
            
            Configuration.SetPinFunction(16, DeviceFunction.COM2_TX);
            Configuration.SetPinFunction(17, DeviceFunction.COM2_RX);

            CancellationTokenSource cts = new CancellationTokenSource();
            DeviceClient client = new DeviceClient(
                cfg.IotHub, 
                cfg.DeviceID, 
                cfg.SASKey,
                nanoFramework.M2Mqtt.Messages.MqttQoSLevel.AtLeastOnce,
                new(IoTHuBCert.AzureChinaRootCA),
                cfg.ModelID);
            client.TwinUpdated += Client_TwinUpdated;
            Debug.WriteLine($"Connecting to iothub [{cfg.IotHub}] - [{cfg.DeviceID}]");
            if (client.Open())
            {
                Debug.WriteLine("Connected to iothub");
                client.GetTwin();
            }
            else
            {
                Debug.WriteLine("Failed connect to iothub");
            }
            StartMainLoop(cts.Token,client);
            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }

        private static void StartMainLoop(CancellationToken token,DeviceClient client)
        {
            SolarChargeController controller = SolarChargeController.OpenDevice("COM2");
            Thread t = new(() =>
            {

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        DeviceTelemetryMessage msg = new DeviceTelemetryMessage();
                        var panelData = controller.ReadPanel();
                        var loadData = controller.ReadLoad();
                        var s = new DeviceTelemetryMessage.Laststatus();
                        s.Battery_V = controller.ReadBatteryVoltage();
                        s.Load_V = loadData.v;
                        s.Load_A = loadData.a;
                        s.Load_W = loadData.w;
                        s.SolarPanel_V = panelData.v;
                        s.SolarPanel_W = panelData.w;
                        s.SolarPanel_A = panelData.a;
                        msg.LastStatus = s;
                        msg.LastUpdate = DateTime.UtcNow.AddHours(8);
                        Debug.WriteLine(JsonSerializer.SerializeObject(msg));
                        client.SendMessage(JsonSerializer.SerializeObject(msg));
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(10));
                    }
                    
                    
                }
                
            });
            t.Start();
        }

        private static void Client_TwinUpdated(object sender, nanoFramework.Azure.Devices.Shared.TwinUpdateEventArgs e)
        {
            Debug.WriteLine($"Twin updated, value ={e.Twin.ToJson()}");
        }

        static void WaitIP()
        {
            Debug.WriteLine("Wait for IP");
            while (true)
            {
                NetworkInterface ni = NetworkInterface.GetAllNetworkInterfaces()[0];
                bool hasIP = false;
                bool hasDate = false;
                if (ni.IPv4Address != null && ni.IPv4Address.Length > 0)
                {
                    if (ni.IPv4Address[0] != '0')
                    {
                        Debug.WriteLine("Have IP " + ni.IPv4Address);
                        hasIP = true;
                    }
                }
                if (DateTime.UtcNow.Year>2000)
                {
                    Debug.WriteLine($"Have Date {DateTime.UtcNow.AddHours(8)}");
                    hasDate = true;
                }
                if (hasIP && hasDate)
                {
                    break;
                }
                Thread.Sleep(1000);
            }
        }
    }
}
