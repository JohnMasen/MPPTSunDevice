using System;
using System.Device.Wifi;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using Device_ESP32C3.Services;
using Device_ESP32C3.Web;
using Iot.Device.DhcpServer;
using Microsoft.Extensions.Logging;
using nanoFramework.Azure.Devices.Client;
using nanoFramework.Azure.Devices.Shared;
using nanoFramework.DependencyInjection;
using nanoFramework.Hardware.Esp32;
using nanoFramework.Hosting;
using nanoFramework.Json;
using nanoFramework.Logging;
using nanoFramework.Logging.Debug;
using nanoFramework.Logging.Stream;
using nanoFramework.Networking;
using nanoFramework.Runtime.Native;
using nanoFramework.WebServer;

namespace Device_ESP32C3
{
    public class Program
    {
        static DateTime turnDCOnUntil = DateTime.MinValue;
        static float minV = 12f;
        //static ILogger log;
        static TcpClient tcpClient;
        public static void Main()
        {
            //OldMain();
            WaitIP();
            ConfigService config = new ConfigService();

            DebugLogger logger = new DebugLogger("SCCD");
            //create test host
            var host = Host.CreateDefaultBuilder().ConfigureServices
                        (cfg =>
                        {
                            cfg.AddSingleton(typeof(ILogger), logger);
                            cfg.AddSingleton(typeof(DeviceRunningConfigService), DeviceRunningConfigService.Default);
                            cfg.AddSingleton(typeof(ConfigService),config);
                            cfg.AddHostedService(typeof(DeviceControlService));
                            cfg.AddSingleton(typeof(SCCDeviceService));
                            cfg.AddHostedService(typeof(IotClientService));
                            //cfg.AddHostedService(typeof(RandomRunningConfigService));
                        })
                        .Build();
            WebServer webServer = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(DischargeController), typeof(SCCStatusController), typeof(TestController) });
            webServer.Start();
            Debug.WriteLine("Web Server Started");
            host.Run();
        }

        private static void OldMain()
        {
            Configuration.SetPinFunction(0, DeviceFunction.COM2_TX);
            Configuration.SetPinFunction(1, DeviceFunction.COM2_RX);
            SolarChargeController device = SolarChargeController.OpenDevice("COM2");


            //must set wifi info in device config
            WaitIP();
            //setup tcp logger
            tcpClient = new TcpClient();
            //try
            //{

            try
            {
                tcpClient.Connect("192.168.0.254", 8234);
            }
            catch (Exception)
            {

            }

            //LogDispatcher.LoggerFactory = new StreamLoggerFactory(tcpClient.GetStream());
            //}
            //catch (Exception)
            //{
            //    LogDispatcher.LoggerFactory = new DebugLoggerFactory();
            //}


            //log = LogDispatcher.GetLogger("Default");

            //Debug.WriteLine(DateTime.UtcNow.AddHours(8).ToString());


            SaveLog("Device Start");
            X509Certificate cert = new X509Certificate(IoTHuBCert.AzureChinaRootCA);
            DeviceClient iotClient = new DeviceClient("johnsolar.azure-devices.cn", "MPPT20A", "z8OieHxMIeonNybJragPZmG0BqANhsbFR3NNiujhfI4=", azureCert: cert, modelId: "dtmi:solar_charge_controller;1");
            CancellationTokenSource cts = new CancellationTokenSource();
            iotClient.TwinUpdated += IotClient_TwinUpdated;
            SaveLog("Connecting to IoTHub...");
            //TODO: handle open timeout
            iotClient.Open();
            SaveLog("IotHub connected,reading twin...");
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    var ts = new CancellationTokenSource(2000);
                    var t = iotClient.GetTwin(ts.Token);
                    if (t.Properties.Desired.Contains("MinVoltage"))
                    {
                        float.TryParse(t.Properties.Desired["MinVoltage"].ToString(), out minV);
                        SaveLog($"MinV={minV},Twin={t.ToJson()}");
                        break;
                    }

                }
                catch (Exception ex)
                {
                    SaveLog(ex.ToString());
                }
            }



            iotClient.AddMethodCallback(TurnOnOutput);

            while (!cts.Token.IsCancellationRequested)
            {
                var value = readValues(device);
                if (value.Battery_V < 12 && device.IsDCOutput)
                {
                    device.SetManualOutput(false);
                    turnDCOnUntil = DateTime.MinValue;
                    SaveLog("Battery low, turn DC Output off");
                }
                if (turnDCOnUntil > DateTime.UtcNow && !device.IsDCOutput)
                {
                    device.SetManualOutput(true);
                    SaveLog("DC On requested, turn on DC output");
                }
                if (turnDCOnUntil <= DateTime.UtcNow && device.IsDCOutput)
                {
                    device.SetManualOutput(false);
                    turnDCOnUntil = DateTime.MinValue;
                    SaveLog("DC on timed out, turn off DC output");
                }
                try
                {
                    if (!iotClient.IsConnected)
                    {
                        iotClient.Reconnect();
                    }
                    iotClient.SendMessage(JsonConvert.SerializeObject(value));
                    SaveLog($"{DateTime.UtcNow.AddHours(8):s} Device message sent");
                }
                catch (Exception)
                {

                }

                Thread.Sleep(1000 * 10);
            }
        }

        private static string TurnOnOutput(int rid, string payload)
        {
            TurnOnOutputParameters p = JsonConvert.DeserializeObject(payload, typeof(TurnOnOutputParameters)) as TurnOnOutputParameters;
            turnDCOnUntil = p.ReadDurationAsDatetime();
            //TODO:Add TunOnUntil to twin
            return string.Empty;
        }

        private static void IotClient_TwinUpdated(object sender, nanoFramework.Azure.Devices.Shared.TwinUpdateEventArgs e)
        {

            SaveLog($"Twin updated twin={e.Twin.ToJson()}");
            if (e.Twin.Contains("MinVoltage"))
            {
                float.TryParse(e.Twin["MinVoltage"].ToString(), out minV);
            }
            TwinCollection r = new() { { "MinVoltage", minV } };
            ((DeviceClient)sender).UpdateReportedProperties(r);

        }

        private static void SaveLog(string log)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(log);
            byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString());
            try
            {
                tcpClient.GetStream().Write(buffer, 0, buffer.Length);
                tcpClient.GetStream().Flush();
            }
            catch (Exception)
            {

            }

        }

        private static DeviceTelemetryMessage readValues(SolarChargeController device)
        {

            DeviceTelemetryMessage result = new DeviceTelemetryMessage();
            var panel = device.ReadPanel();
            result.SolarPanel_V = panel.v;
            result.SolarPanel_W = panel.w;
            result.SolarPanel_A = panel.a;

            var l = device.ReadLoad();
            result.Load_A = l.a;
            result.Load_V = l.v;
            result.Load_W = l.w;

            result.Battery_V = device.ReadBatteryVoltage();
            return result;
        }
        #endregion


        static void WaitIP()
        {
            Debug.WriteLine("Wait for IP");
            while (true)
            {
                NetworkInterface ni = NetworkInterface.GetAllNetworkInterfaces()[0];
                if (ni.IPv4Address != null && ni.IPv4Address.Length > 0)
                {
                    if (ni.IPv4Address[0] != '0')
                    {
                        Debug.WriteLine("Have IP " + ni.IPv4Address);
                        break;
                    }
                }
                Thread.Sleep(1000);
            }
        }


    }
}
