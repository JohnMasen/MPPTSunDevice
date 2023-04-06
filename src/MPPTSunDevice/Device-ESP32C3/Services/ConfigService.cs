using nanoFramework.Hosting;
using System;
using System.Text;

namespace Device_ESP32C3.Services
{
    internal class ConfigService
    {
        public class IotClientConfig
        {
            public string HostName;
            public string DeviceName;
            public string SAS;
            public string ModelID;
        }
        public int Pin_TX { get; private set; }
        public int Pin_RX { get; private set; }
        public string COMPort { get; set; }

        public IotClientConfig IotConfig { get; set; }

        /// <summary>
        /// interval of read data from SCC device (unit:seconds), default =10
        /// </summary>
        public int DeviceReadInterval { get; set; } = 10;
        public ConfigService(int defaultTXPin=0, int defaultRXPin=1,string defaultCOM="COM2")
        {
            Pin_TX = defaultTXPin;
            Pin_RX= defaultRXPin;
            COMPort = defaultCOM;
            IotConfig = new IotClientConfig();
        }
       

       
    }
}
