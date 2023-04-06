using System;
using System.Text;

namespace Device_ESP32C3
{
    internal class TurnOnOutputParameters
    {
        public int duration { get; set; }
        public DateTime ReadDurationAsDatetime()
        {
            if (duration >= 0)
            {
                return DateTime.UtcNow.AddMinutes(duration);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(duration));
            }

        }
    }
}
