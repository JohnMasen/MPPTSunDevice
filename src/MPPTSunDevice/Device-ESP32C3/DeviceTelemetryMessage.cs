using System;
using System.Text;

namespace Device_ESP32C3
{
    internal class DeviceTelemetryMessage
    {
        public float SolarPanel_V { get; set; }
        public float SolarPanel_A { get; set; }
        public float SolarPanel_W { get; set; }
        public float Battery_V { get; set; }
        public float Load_V { get; set; }
        public float Load_A { get; set; }
        public float Load_W { get; set; }
    }
}
