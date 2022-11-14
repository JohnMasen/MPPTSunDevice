using FluentModbus;
using System.Runtime.InteropServices;

namespace MPPTSunDevice
{
    public class SolarChargeController : IDisposable
    {
        private bool disposedValue;

        public string SerialPortName { get; init; }
        private ModbusRtuClient client;

        private SolarChargeController(string serialPortName)
        {
            SerialPortName = serialPortName;
            client = new ModbusRtuClient();
            client.BaudRate = 9600;
            client.Connect(serialPortName,ModbusEndianness.BigEndian);
        }

        public static SolarChargeController OpenDevice(string serialPortName)
        {
            return new SolarChargeController(serialPortName);
        }

        public float ReadBatteryVoltage()
        {
            var data = readData<ushort>(0x0101, 1);
            Console.WriteLine($"length={data.Length}");
            return data[0] * 0.1f;
        }

        public  (float v,float a,float w) ReadLoad()
        {
            var data = readData<ushort>(0x0104, 3);
            return (v: data[0] * 0.1f, a: data[1] * 0.01f, w: data[2]);
        }
        public (float v,float a,float w) ReadPanel()
        {
            var data = readData<ushort>(0x0107, 3);
            return (v: data[0] * 0.1f, a: data[1] * 0.01f, w: data[2] );
        }

        public ushort ReadWorkloadType()
        {
            return readData<ushort>(0xe01d, 1)[0];
        }

        public float ReadBatterySOC()
        {
            return readData<ushort>(0x0100, 1)[0]*0.01f;
        }

        private Span<T> readData<T>(int address,int length) where T : unmanaged
        {
            return client.ReadHoldingRegisters<T>(1, address, length);
        }

        #region Dispose

        
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    client.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SolarChargeController()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion 
    }
}