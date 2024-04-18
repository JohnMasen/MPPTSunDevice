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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using nanoFramework.Azure.Devices.Client;
using nanoFramework.Azure.Devices.Shared;
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
                            //cfg.AddHostedService(typeof(IotClientService));
                            //cfg.AddSingleton(typeof(IoTRunningStatus), IoTRunningStatus.Default);
                            //cfg.AddHostedService(typeof(RandomRunningConfigService));
                        })
                        .Build();
            WebServer webServer = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(DischargeController), typeof(SCCStatusController), typeof(TestController) });
            webServer.Start();
            Debug.WriteLine("Web Server Started");
            host.Run();
        }



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
