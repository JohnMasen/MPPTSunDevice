using System;
using System.IO;
using System.IO.Ports;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Device_ESP32C3.SerialTool
{
    internal class SerialToTCP
    {
        TcpListener server;
        SerialPort localSerial;
        bool isMapped = false;
        byte[] buffer=new byte[1024];
        CancellationTokenSource cts = new CancellationTokenSource();
        public SerialToTCP(SerialPort serialPort,int tcpPort )
        {
            server = new TcpListener(System.Net.IPAddress.Any, tcpPort);
            server.Start(100);
            localSerial= serialPort;
            Thread serverLoop = new Thread(TcpServerRun);
            serverLoop.Start();
        }


        private void TcpServerRun()
        {
            var client=server.AcceptTcpClient();
            if (isMapped)
            {
                client.Close();
            }
            Thread syncLoop = new Thread(() => { streamSync(client.Client, cts.Token); });
        }

        private void streamSync(Socket target,CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                //map data from serial to tcp
                int bytes = localSerial.BytesToRead;
                if (bytes > 0)
                {
                    localSerial.Read(buffer, 0, bytes);
                    target.Send(buffer, bytes, SocketFlags.None);
                }

                //map data from tcp to serial
                if (target.Available>0)
                {
                    int receivedBytes = target.Receive(buffer);
                    localSerial.Write(buffer,0,receivedBytes);
                }
            }
        }
    }
}
