using System;
using System.Text;

namespace Device_ESP32C3.Services
{
    internal class DeviceRunningConfigService
    {
        public float OverDischargeVoltage { get; set; } = 12f;
        public float MaxVoltage { get; set; } = 14.4f;
        public DeviceTelemetryMessage LastStatus { get; set; }
        public DateTime LastUpdate { get; set; }
        public DateTime TurnDCOnUntil { get; set; }

        public void TurnDCOn(TimeSpan duration)
        {
            TurnDCOnUntil = DateTime.UtcNow.Add(duration);
        }

        internal static DeviceRunningConfigService Default = new DeviceRunningConfigService();
        public int FaildReads { get; set; } = 0;

    }
}
