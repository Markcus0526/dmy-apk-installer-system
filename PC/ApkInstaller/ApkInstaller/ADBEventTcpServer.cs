using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Diagnostics;

namespace ApkInstaller
{
    class ADBEventTcpServer
    {
        private bool m_bServerLoop;
        private int m_tcpPort = 51400;

        public delegate void DeviceReportHandler(String sendername, String report);
        public event DeviceReportHandler onDeviceReport;

        private TcpListener listener;

        public void InitServer()
        {
            onDeviceReport += new DeviceReportHandler(Program.mDevicePoll.DeviceReported);
        }

        public void Listen()
        {
            m_bServerLoop = true;
            try
            {
                List<Port> ports = AdbPortCheckUtilites.GetNetStatPorts(m_tcpPort);

                foreach (Port portItem in ports)
                {
                    if (Convert.ToInt32(portItem.port_number) == m_tcpPort && portItem.process_id != 0)
                    {
                        portItem.process_path = AdbPortCheckUtilites.LookupProcessPath(portItem.process_id);
                        Process proc = Process.GetProcessById(portItem.process_id);
                        proc.Kill();
                    }
                }

                listener = new TcpListener(IPAddress.Parse("127.0.0.1"), m_tcpPort);
                listener.Start();
                listener.BeginAcceptTcpClient(WaitForConnectionCallBack, listener);
            }
            catch (Exception oEX)
            {
                Console.WriteLine(oEX.Message);
            }
        }

        private void WaitForConnectionCallBack(IAsyncResult iar)
        {
            try
            {
                TcpClient client = listener.EndAcceptTcpClient(iar);

                using (NetworkStream ns = client.GetStream())
                {
                    byte[] buffer = new byte[4000];
                    int count = ns.Read(buffer, 0, 4000);

                    if (count > 0)
                    {
                        String message = Encoding.UTF8.GetString(buffer, 0, count);

                        string[] msgs = message.Split('|');
                        onDeviceReport(msgs[0], message.Substring(message.IndexOf('|') + 1));
                        Console.WriteLine(message);
                    }
                }

                // Kill original sever and create new wait server
                client.Close();

                if (m_bServerLoop)
                {
                    listener.BeginAcceptTcpClient(WaitForConnectionCallBack, listener);
                }

            }
            catch (Exception oEX)
            {
                Console.Write(oEX.Message);
                return;
            }
        }

        public void RequestStop()
        {
            if (m_bServerLoop)
                m_bServerLoop = false;
            Thread.Sleep(100);

            listener.Stop();
            listener = null;
        }

        public Boolean IsServerRunning()
        {
            return m_bServerLoop;
        }
    }
}
