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

        public IotClientService(ConfigService c,ILogger logger, DeviceRunningConfigService deviceControlConfig)
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
        }

        private void IotClient_TwinUpdated(object sender, nanoFramework.Azure.Devices.Shared.TwinUpdateEventArgs e)
        {
            logger.LogTrace($"device twin updated");
            float minV;
            if (e.Twin.Contains("MinVoltage"))
            {
                if(float.TryParse(e.Twin["MinVoltage"].ToString(), out minV))
                {
                    TwinCollection r = new() { { "MinVoltage", minV } };
                    ((DeviceClient)sender).UpdateReportedProperties(r);
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
            while (!CancellationRequested)
            {
                //connect to IoTHuB if not connected, reigster device twin update callback
                if (!iotClient.IsConnected)
                {
                    iotClient.Open();
                    CancellationTokenSource cts = new CancellationTokenSource(10000);
                    try
                    {
                        iotClient.GetTwin(cts.Token);
                    }
                    catch (Exception)
                    {

                    }
                    
                }
                
                //update device twin

                //iothub connection watchdog init




                Thread.Sleep(10000);//
            }
        }
        private void handleDeviceTwin(Twin data)
        {
            float minV;
            bool minVValid = false;
            if (data.Properties.Desired.Contains("MinVoltage"))
            {
                if(float.TryParse(data.Properties.Desired["MinVoltage"].ToString(), out minV))
                {
                    deviceConfig.OverDischargeVoltage = minV;
                }
                logger.LogInformation($"MinV={minV},Twin={data.ToJson()}");
            }
        }
        private void StartWatchDog(int timeout, Func<bool> callbacl)
        {
            
        }
    }
}
