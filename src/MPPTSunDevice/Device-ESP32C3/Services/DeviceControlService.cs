using Microsoft.Extensions.Logging;
using nanoFramework.Hardware.Esp32;
using nanoFramework.Hosting;
using nanoFramework.Runtime.Native;
using System;
using System.Text;

namespace Device_ESP32C3.Services
{
    internal class DeviceControlService : SchedulerService
    {
        private readonly DeviceRunningConfigService running;
        private readonly SCCDeviceService deviceService;
        private readonly ILogger logger;
        private const int MAX_FAIL_COUNT = 3;

        private int failCount = 0;
        public DeviceControlService(SCCDeviceService sCCDevice,DeviceRunningConfigService configService,ILogger loggerService ) : base(TimeSpan.FromSeconds(1))
        {
            deviceService = sCCDevice;
            running = configService;
            logger = loggerService;
        }

        protected override void ExecuteAsync()
        {
            //read device status
            try
            {
                running.LastStatus = readStatus();
                running.LastUpdate = DateTime.UtcNow;
                failCount = 0;
            }
            catch (Exception)
            {
                failCount++;
                if (failCount==MAX_FAIL_COUNT)
                {
                    Power.RebootDevice();
                    
                }
                return;
            }

            //disable DC output if battery is over discharged
            if (running.LastStatus.Battery_V<=running.OverDischargeVoltage && deviceService.Device.IsDCOutput)
            {
                deviceService.Device.SetManualOutput(false);
                logger.LogInformation("Battery power low, turn off DC output");
                running.TurnDCOnUntil = DateTime.MinValue;
            }
            if (running.TurnDCOnUntil>DateTime.UtcNow && deviceService.Device.IsDCOutput==false)
            {
                deviceService.Device.SetManualOutput(true);
                logger.LogInformation("Turn DC requested, turning DC on");
            }
            if (running.TurnDCOnUntil<=DateTime.UtcNow && deviceService.Device.IsDCOutput)
            {
                deviceService.Device.SetManualOutput(false);
                logger.LogInformation("DC output timedout, turning DC off");
            }


        }

        private DeviceTelemetryMessage readStatus()
        {
            DeviceTelemetryMessage result = new DeviceTelemetryMessage();
            var panel = deviceService.Device.ReadPanel();
            result.SolarPanel_V = panel.v;
            result.SolarPanel_W = panel.w;
            result.SolarPanel_A = panel.a;

            var l = deviceService.Device.ReadLoad();
            result.Load_A = l.a;
            result.Load_V = l.v;
            result.Load_W = l.w;

            result.Battery_V = deviceService.Device.ReadBatteryVoltage();
            return result;
        }
    }
}
