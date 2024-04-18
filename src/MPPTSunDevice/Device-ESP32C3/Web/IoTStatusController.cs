using Device_ESP32C3.Services;
using nanoFramework.Json;
using nanoFramework.WebServer;
using System;
using System.Text;

namespace Device_ESP32C3.Web
{
    internal class IoTStatusController
    {
        [Route("/api/getIoTStatus")]
        [CaseSensitive]
        [Method("GET")]
        public void GetStatus(WebServerEventArgs e)
        {
            string r = JsonSerializer.SerializeObject(IoTRunningStatus.Default);
            e.Context.Response.ContentType = "application/json";
            e.Context.Response.Headers.Add("Cache-Control", "no-cache");
            WebServer.OutPutStream(e.Context.Response, r);
        }
    }
}
