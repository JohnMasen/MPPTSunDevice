using Device_ESP32C3.Services;
using nanoFramework.Json;
using nanoFramework.WebServer;
using System;
using System.Text;

namespace Device_ESP32C3.Web
{
    internal class SCCStatusController
    {
        [Route("/api/getStatus")]
        [CaseSensitive]
        [Method("GET")]
        public void GetStatus(WebServerEventArgs e)
        {
            string r = JsonSerializer.SerializeObject(DeviceRunningConfigService.Default);
            e.Context.Response.ContentType="application/json";
            WebServer.OutPutStream(e.Context.Response, r);
        }
    }
}
