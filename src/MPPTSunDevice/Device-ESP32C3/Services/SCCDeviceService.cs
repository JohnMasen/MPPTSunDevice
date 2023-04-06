using Microsoft.Extensions.Logging;
using nanoFramework.Hardware.Esp32;
using nanoFramework.Hosting;
using System;
using System.Text;

namespace Device_ESP32C3.Services
{
    internal class SCCDeviceService 
    {
        private ConfigService config;
        private ILogger logger;
        public SCCDeviceService(ConfigService config,ILogger logger)
        {
            this.config = config;
            this.logger = logger;
            Configuration.SetPinFunction(config.Pin_TX, DeviceFunction.COM2_TX);
            Configuration.SetPinFunction(config.Pin_RX, DeviceFunction.COM2_RX);
            Device = SolarChargeController.OpenDevice(config.COMPort);
        }
        public SolarChargeController Device { private set; get; }
        
    }
}
