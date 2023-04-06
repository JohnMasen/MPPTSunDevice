using System;
using System.Text;

namespace Device_ESP32C3
{
    internal static class DateTimeHelper
    {
        internal static DateTime toLocal(this DateTime value)
        {
            return value.AddHours(8);
        }
    }
}
