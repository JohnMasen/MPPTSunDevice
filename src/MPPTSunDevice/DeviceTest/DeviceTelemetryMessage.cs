using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTest
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
