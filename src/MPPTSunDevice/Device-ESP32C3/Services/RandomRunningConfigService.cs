using nanoFramework.Hosting;
using System;
using System.Text;

namespace Device_ESP32C3.Services
{
    internal class RandomRunningConfigService:SchedulerService
    {
        private DeviceRunningConfigService configService { get; init; }
        public RandomRunningConfigService(DeviceRunningConfigService configService) : base(TimeSpan.FromSeconds(1))
        {

            this.configService = configService;

        }

        protected override void ExecuteAsync()
        {
            Random r = new Random();
            
            configService.LastUpdate = DateTime.UtcNow;
            configService.LastStatus = new DeviceTelemetryMessage()
            {
                 Battery_V=(float)r.NextDouble(),
                 Load_A=(float)r.NextDouble(),
                 Load_V=(float)r.NextDouble(),
                 Load_W=(float)r.NextDouble(),
                 SolarPanel_A=(float)r.NextDouble(),
                 SolarPanel_V=(float)r.NextDouble(),
                 SolarPanel_W=(float)r.NextDouble()
            };
        }
    }
}
