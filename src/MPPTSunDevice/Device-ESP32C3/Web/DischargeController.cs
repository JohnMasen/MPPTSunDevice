using Device_ESP32C3.Services;
using nanoFramework.WebServer;
using System;
using System.Text;

namespace Device_ESP32C3.Web
{
    internal class DischargeController
    {
        [Route("/api/discharge")]
        [Method("GET")]
        public void Discharge(WebServerEventArgs e)
        {
            var paras=WebServer.DecodeParam(e.Context.Request.RawUrl);
            foreach (var item in paras)
            {
                if (item.Name.ToLower()=="duration")
                {
                    if (int.TryParse(item.Value,out int d))
                    {
                        DeviceRunningConfigService.Default.TurnDCOn(TimeSpan.FromMinutes(d));
                    }
                }
            }
        }
    }
}
