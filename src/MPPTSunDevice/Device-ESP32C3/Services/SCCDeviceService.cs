using nanoFramework.Hosting;
using System;
using System.Text;

namespace Device_ESP32C3.Services
{
    internal class SCCDeviceService : IHostedService
    {
        private ConfigService config;
        public SCCDeviceService(ConfigService config)
        {
            this.config = config;   
        }
        public SolarChargeController Device { private set; get; }
        public void Start()
        {
            Device = SolarChargeController.OpenDevice(config.COMPort);
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
