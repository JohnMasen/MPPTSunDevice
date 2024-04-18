using System;
using System.Text;

namespace Device_ESP32C3.Services
{
    internal class IoTRunningStatus
    {
        public bool IsConnected { get; set; }
        public DateTime LastConnected { get; set; }
        public DateTime LastReportTime { get; set; }
        public static IoTRunningStatus Default = new IoTRunningStatus();
    }
}
