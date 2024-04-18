using Microsoft.Extensions.Logging;
using nanoFramework.Azure.Devices.Client;
using nanoFramework.Azure.Devices.Shared;
using nanoFramework.Hardware.Esp32;
using nanoFramework.Hosting;
using nanoFramework.Json;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace Device_ESP32C3.Services
{
    internal class IotClientService : BackgroundService
    {
        ConfigService config;
        DeviceClient iotClient;
        ILogger logger { get; init; }
        DeviceRunningConfigService deviceConfig;
        IoTRunningStatus iotStatus;

        public IotClientService(ConfigService c,ILogger logger, DeviceRunningConfigService deviceControlConfig,IoTRunningStatus ioTRunningStatus)
        {
            config = c;
            iotClient = new DeviceClient(
                c.IotConfig.HostName,
                c.IotConfig.DeviceName,
                c.IotConfig.SAS,
                nanoFramework.M2Mqtt.Messages.MqttQoSLevel.AtLeastOnce,
                new X509Certificate(IoTHuBCert.AzureChinaRootCA),
                c.IotConfig.ModelID);
            iotClient.AddMethodCallback(IoTMethodCallback);
            iotClient.TwinUpdated += IotClient_TwinUpdated;
            this.logger= logger;
            deviceConfig = deviceControlConfig;
            iotStatus = ioTRunningStatus;
        }

        private void IotClient_TwinUpdated(object sender, nanoFramework.Azure.Devices.Shared.TwinUpdateEventArgs e)
        {
            logger.LogTrace($"device twin updated");
            handleTwin(iotClient, e.Twin);
        }
        private void handleTwin(DeviceClient device, TwinCollection data)
        {
            float minV;
            if (data.Contains("MinVoltage"))
            {
                if (float.TryParse(data["MinVoltage"].ToString(), out minV))
                {
                    TwinCollection r = new() { { "MinVoltage", minV } };
                    device.UpdateReportedProperties(r);
                    deviceConfig.OverDischargeVoltage = minV;
                }

            }
        }

        private string IoTMethodCallback(int rid, string payload)
        {
            TurnOnOutputParameters p = JsonConvert.DeserializeObject(payload, typeof(TurnOnOutputParameters)) as TurnOnOutputParameters;
            deviceConfig.TurnDCOnUntil = p.ReadDurationAsDatetime();
            return string.Empty;
        }

        protected override void ExecuteAsync()
        {
            StartWatchDog();
            while (!CancellationRequested)
            {
                iotStatus.IsConnected = iotClient.IsConnected;
                //connect to IoTHuB if not connected
                if (!iotClient.IsConnected)
                {
                    iotClient.Open();
                    
                    for (int i = 0; i < 3; i++)
                    {
                        CancellationTokenSource cts = new CancellationTokenSource(10000);
                        try
                        {
                            var data = iotClient.GetTwin(cts.Token);
                            handleTwin(iotClient, data.Properties.Desired);
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
                iotStatus.IsConnected = iotClient.IsConnected;
                //update device twin

                //iothub connection watchdog init

                //upload iot data
                try
                {
                    if ((deviceConfig.LastUpdate-DateTime.UtcNow).TotalSeconds<10)
                    {
                        string msg = JsonSerializer.SerializeObject(deviceConfig.LastStatus);
                        iotClient.SendMessage(msg);
                        DateTime localNow = DateTime.UtcNow.AddHours(8);
                        iotStatus.LastReportTime = localNow;
                        iotStatus.LastConnected = localNow;
                    }
                }
                catch (Exception)
                {
                    iotClient.Close();
                    iotStatus.IsConnected = iotClient.IsConnected;
                }
                

                Thread.Sleep(10000);//
            }
        }

        private void StartWatchDog()
        {
            
        }
    }
}
