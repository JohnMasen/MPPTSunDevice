using Microsoft.Extensions.Logging;
using nanoFramework.Hardware.Esp32;
using nanoFramework.Hosting;
using nanoFramework.Runtime.Native;
using System;
using System.Text;
using System.Threading;

namespace Device_ESP32C3.Services
{
    internal class DeviceControlService : BackgroundService
    {
        private readonly DeviceRunningConfigService running;
        private readonly SCCDeviceService deviceService;
        private readonly ILogger logger;
        //private const int MAX_FAIL_COUNT = 3;

        //private int failCount = 0;
        public DeviceControlService(SCCDeviceService sCCDevice, DeviceRunningConfigService configService, ILogger loggerService)
        {
            deviceService = sCCDevice;
            running = configService;
            logger = loggerService;
            ShutdownTimeout = TimeSpan.FromSeconds(3);
        }

        protected override void ExecuteAsync()
        {
            while (!CancellationRequested)
            {
                //read device status
                try
                {
                    running.LastStatus = readStatus();
                    running.LastUpdate = DateTime.UtcNow;
                    //failCount = 0;
                    if (running.LastStatus == null)
                    {
                        running.FaildReads++;
                    }
                }
                catch (Exception)
                {
                    running.FaildReads++;
                }
                if (running.LastStatus != null)// the device may randomly return error response, modbus library will return null when CRC error
                {
                    //disable DC output if battery is over discharged
                    if (running.LastStatus.Battery_V <= running.OverDischargeVoltage && deviceService.Device.IsDCOutput)
                    {
                        deviceService.Device.SetManualOutput(false);
                        logger.LogInformation("Battery power low, turn off DC output");
                        running.TurnDCOnUntil = DateTime.MinValue;
                    }
                    if (running.TurnDCOnUntil > DateTime.UtcNow && deviceService.Device.IsDCOutput == false)
                    {
                        deviceService.Device.SetManualOutput(true);
                        logger.LogInformation("Turn DC requested, turning DC on");
                    }
                    if (running.TurnDCOnUntil > DateTime.MinValue && running.TurnDCOnUntil <= DateTime.UtcNow && deviceService.Device.IsDCOutput)
                    {
                        deviceService.Device.SetManualOutput(false);
                        logger.LogInformation("DC output timedout, turning DC off");
                    }
                }

                Thread.Sleep(2000);
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
