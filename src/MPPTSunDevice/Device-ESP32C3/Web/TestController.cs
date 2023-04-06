using nanoFramework.WebServer;
using System;
using System.Diagnostics;
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
            e.Context.Response.StatusCode = 200;
            e.Context.Response.ContentType = "text/HTML; charset=utf-8";
            e.Context.Response.ContentEncoding= Encoding.UTF8;
            WebServer.OutPutStream(e.Context.Response, WebResource.GetString(WebResource.StringResources.Index));
        }
    }
}
