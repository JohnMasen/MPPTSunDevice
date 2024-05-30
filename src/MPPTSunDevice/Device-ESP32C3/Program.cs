using Device_ESP32C3.Services;
using Device_ESP32C3.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using nanoFramework.Hosting;
using nanoFramework.Logging.Debug;
using nanoFramework.WebServer;
using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

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
            config.IotConfig.HostName = "johnsolar.azure-devices.cn";
            config.IotConfig.DeviceName = "MPPT20ANano";
            config.IotConfig.SAS = "XkejEweQBgqOOOodyyo5IM710avTbri4VAIoTGnpKzo=";
            config.IotConfig.ModelID = "dtmi:solar_charge_controller;1";

            DebugLogger logger = new DebugLogger("SCCD");
            //create test host
            var host = Host.CreateDefaultBuilder().ConfigureServices
                        (cfg =>
                        {
                            cfg.AddSingleton(typeof(ILogger), logger);
                            cfg.AddSingleton(typeof(DeviceRunningConfigService), DeviceRunningConfigService.Default);
                            cfg.AddSingleton(typeof(ConfigService), config);
                            cfg.AddHostedService(typeof(DeviceControlService));
                            cfg.AddSingleton(typeof(SCCDeviceService));
                            cfg.AddHostedService(typeof(IotClientService));
                            cfg.AddSingleton(typeof(IoTRunningStatus), IoTRunningStatus.Default);
                            //cfg.AddHostedService(typeof(RandomRunningConfigService));
                        })
                        .Build();
            //WebServer webServer = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(DischargeController), typeof(SCCStatusController)
            //, typeof(TestController)
            //});
            //webServer.Start();
            //Debug.WriteLine("Web Server Started");
            host.Run();

            //Thread.Sleep(Timeout.Infinite);
        }
        private void RunTCPMapper()
        {
            SerialTool.SerialToTCP st = new SerialTool.SerialToTCP(new SerialPort("COM2"), 8000);
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
