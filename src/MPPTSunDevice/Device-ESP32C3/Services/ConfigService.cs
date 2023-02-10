using nanoFramework.Hosting;
using System;
using System.Text;

namespace Device_ESP32C3.Services
{
    internal class ConfigService:IHostedService
    {
        public int Pin_TX { get; private set; }
        public int Pin_RX { get; private set; }
        public string COMPort { get; set; }
        public ConfigService(int defaultTXPin=0, int defaultRXPin=1,string defaultCOM="COM2")
        {
            Pin_TX = defaultTXPin;
            Pin_RX= defaultRXPin;
            COMPort = defaultCOM;
        }
       

        public void Start()
        {
            
        }

        public void Stop()
        {
            
        }
    }
}
