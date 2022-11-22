using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DeviceTest
{
    internal class SetOutputMethodParameters
    {
        public string value { get; set; }
        public bool IsOn=>bool.Parse(value);

    }
}
