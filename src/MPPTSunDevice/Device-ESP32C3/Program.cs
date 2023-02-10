using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Device_ESP32C3.Services;
using Device_ESP32C3.Web;
using Iot.Device.DhcpServer;
using nanoFramework.DependencyInjection;
using nanoFramework.Hardware.Esp32;
using nanoFramework.Hosting;
using nanoFramework.Runtime.Native;
using nanoFramework.WebServer;

namespace Device_ESP32C3
{
    public class Program
    {
        public static void Main()
        {

            Configuration.SetPinFunction(0, DeviceFunction.COM2_TX);
            Configuration.SetPinFunction(1, DeviceFunction.COM2_RX);
            EnableAP();
            using var webServer=webServerTest();
            test();


            //Host.CreateDefaultBuilder()
            //    .ConfigureServices(services =>
            //    {
            //        services
            //            .AddSingleton(typeof(SolarChargeController))
            //            .AddSingleton(typeof(ConfigService), new ConfigService());
            //    }).Build().Run();


            Thread.Sleep(Timeout.Infinite);
               
        }
        private static void test()
        {
            SolarChargeController device = SolarChargeController.OpenDevice("COM2");
            var xx = device.ReadPanel();
        }

        private static IDisposable webServerTest()
        {
            WebServer webServer = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(TestController) });
            webServer.Start();
            return webServer;
        }

        private static void EnableAP()
        {
            if (Wireless80211.IsEnabled())
            {
                Wireless80211.Disable();
            }
            if (!WirelessAP.Setup())
            {
                Debug.WriteLine($"Setup Soft AP, Rebooting device");
                Power.RebootDevice();
            }
            var dhcpServer = new DhcpServer();
            var dhcpInitResult = dhcpServer.Start(IPAddress.Parse(WirelessAP.SoftApIP), new IPAddress(new byte[] { 255, 255, 255, 0 }));
            if (!dhcpInitResult)
            {
                Debug.WriteLine($"Error initializing DHCP server.");
            }
            else
            {
                Debug.WriteLine($"Running Soft AP, waiting for client to connect");
                Debug.WriteLine($"Soft AP IP address :{WirelessAP.GetIP()}");
            }

        }
    }
}
