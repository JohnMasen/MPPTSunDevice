using nanoFramework.WebServer;
using System;
using System.Net;
using System.Text;

namespace Device_ESP32C3.Web
{

    public class TestController
    {


        [Route("/")]
        [Method("GET")]
        public void DefaultGet(WebServerEventArgs e)
        {
            string ret = "This is test";
            e.Context.Response.StatusCode = 200;
            e.Context.Response.ContentType = "text/plain";
            WebServer.OutPutStream(e.Context.Response,ret);
        }
    }
}
