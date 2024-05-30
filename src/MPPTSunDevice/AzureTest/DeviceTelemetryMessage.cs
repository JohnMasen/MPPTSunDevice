using System;
using System.Text;

namespace AzureTest
{
    public  class DeviceTelemetryMessage
    {
        public DateTime TurnDCOnUntil { get; set; }
        public DateTime LastUpdate { get; set; }
        public float MaxVoltage { get; set; }
        public float OverDischargeVoltage { get; set; }
        public Laststatus LastStatus { get; set; }
        public  class Laststatus
        {
            public float Load_V { get; set; }
            public float SolarPanel_V { get; set; }
            public float Load_A { get; set; }
            public float Battery_V { get; set; }
            public float SolarPanel_A { get; set; }
            public float Load_W { get; set; }
            public float SolarPanel_W { get; set; }
        }
    }
}
