using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTest
{
    internal class TurnOnOutputParameters
    {
        public int duration { get; set; }
        public DateTime? ReadDurationAsDatetime()
        {
            if (duration >= 0)
            {
                return DateTime.Now.AddMinutes(duration);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(duration));
            }

        }
    }
}
