using FluentModbus;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;

namespace MPPTSunDevice
{
    public class SolarChargeController : IDisposable
    {
        private bool disposedValue;

        public string SerialPortName { get; init; }
        private ModbusRtuClient client;
        private DateTime? lastRun = null;
        public enum WorkloadType : ushort
        {
            Delay_0 = 0,
            Delay_1 = 1,
            Delay_2 = 2,
            Delay_3 = 3,
            Delay_4 = 4,
            Delay_5 = 5,
            Delay_6 = 6,
            Delay_7 = 7,
            Delay_8 = 8,
            Delay_9 = 9,
            Delay_10 = 10,
            Delay_11 = 11,
            Delay_12 = 12,
            Delay_13 = 13,
            Delay_14 = 14,
            Manual = 15,
            Debug = 16,
            AlwaysOn = 17
        }
        private SolarChargeController(string serialPortName)
        {
            SerialPortName = serialPortName;
            client = new ModbusRtuClient();
            client.BaudRate = 9600;
            client.Connect(serialPortName, ModbusEndianness.BigEndian);
        }

        public static SolarChargeController OpenDevice(string serialPortName)
        {
            return new SolarChargeController(serialPortName);
        }

        public float ReadBatteryVoltage()
        {
            var data = readData<ushort>(0x0101, 1);
            return data[0] * 0.1f;
        }

        public (float v, float a, float w) ReadLoad()
        {
            var data = readData<ushort>(0x0104, 3);
            return (v: data[0] * 0.1f, a: data[1] * 0.01f, w: data[2]);
        }
        public (float v, float a, float w) ReadPanel()
        {
            var data = readData<ushort>(0x0107, 3);
            return (v: data[0] * 0.1f, a: data[1] * 0.01f, w: data[2]);
        }

        public ushort ReadWorkloadType()
        {
            return readData<ushort>(0xe01d, 1)[0];
        }

        public void WriteWorkloadType(WorkloadType type)
        {
            writeData(0xe01d, (ushort)type);
            Thread.Sleep(10);
            if ((ushort)type != ReadWorkloadType())
            {
                throw new InvalidOperationException("Workload change failed");
            }
        }


        /// <summary>
        /// Change the DC output to On/Off
        /// </summary>
        /// <param name="isOutput">turn on/off DC output</param>
        /// <param name="forceSet">change output mode to manual if required</param>
        public void SetManualOutput(bool isOutput, bool forceSet = true)
        {
            if (ReadWorkloadType() != 15)
            {
                if (forceSet)
                {
                    WriteWorkloadType(WorkloadType.Manual);
                }
                else
                {
                    throw new InvalidOperationException("Could not set output mode, call WriteWorkloadType set output work mode to [Manual] first");
                }
            }
            client.WriteSingleRegister(1, 0x010a, (ushort)(isOutput ? 1 : 0));
        }

        public float ReadBatterySOC()
        {
            return readData<ushort>(0x0100, 1)[0] * 0.01f;
        }

        private Span<T> readData<T>(int address, int length) where T : unmanaged
        {
            lock (client)
            {
                waitForDeviceReady();
                return client.ReadHoldingRegisters<T>(1, address, length);
            }
            
        }

        private void writeData(int address,ushort data) 
        {
            lock (client)
            {
                waitForDeviceReady();
                client.WriteSingleRegister(1, address, data);
            }
            
        }

        #region Dispose

        public bool IsDCOutput => (readData<ushort>(0x0120, 1)[0] & 0b_1000_0000_0000_0000) >0;

        private void waitForDeviceReady()
        {
            if (lastRun.HasValue && (lastRun.Value-DateTime.Now).TotalMilliseconds<10)
            {
                Thread.Sleep(10);
            }
        }
        
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