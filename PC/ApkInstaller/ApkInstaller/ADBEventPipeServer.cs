using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace ApkInstaller
{
    class ADBEventPipeServer
    {
        private bool m_bServerLoop;
        private String m_strServername="noname";

        public delegate void DeviceReportHandler(String sendername, String report);
        public event DeviceReportHandler onDeviceReport;

        public NamedPipeServerStream m_pipeServer;
        public void InitServer(String servername)
        {
            m_strServername = servername;
            onDeviceReport += new DeviceReportHandler(Program.mDevicePoll.DeviceReported);
            m_pipeServer = new NamedPipeServerStream(m_strServername,
               PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        }

        public void Listen()
        {
            m_bServerLoop = true;
            try
            {
                // Set to class level var so we can re-use in the async callback method
              
                // Create the new async pipe 

                // Wait for a connection
                m_pipeServer.BeginWaitForConnection
                (new AsyncCallback(WaitForConnectionCallBack), m_pipeServer);
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
                // Get the pipe
                NamedPipeServerStream pipeServer = (NamedPipeServerStream)iar.AsyncState;
                // End waiting for the connection

                pipeServer.EndWaitForConnection(iar);

                byte[] buffer = new byte[65535];

                Int32 count = pipeServer.Read(buffer, 0, 65535);

                if (count > 0)
                {
//                     UTF8Encoding encoding = new UTF8Encoding();
//                     String message = encoding.GetString(buffer, 0, count);
                    String message = Encoding.UTF8.GetString(buffer, 0, count);

                    string[] msgs = message.Split('|');
                    onDeviceReport(msgs[0], message.Substring( message.IndexOf('|')+1 ));
                    Console.WriteLine(message);
                }

                // Kill original sever and create new wait server
                m_pipeServer.Close();
                m_pipeServer = null;

                if (m_bServerLoop)
                {
                    m_pipeServer = new NamedPipeServerStream(m_strServername, PipeDirection.InOut,
                   1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

                    // Recursively wait for the connection again and again....
                    m_pipeServer.BeginWaitForConnection(
                       new AsyncCallback(WaitForConnectionCallBack), m_pipeServer);
                }
            }
            catch (Exception oEX)
            {
                Console.Write(oEX.Message);

//                 if (m_pipeServer != null)
//                 {
//                     m_pipeServer.Close();
//                     m_pipeServer = null;
//                 }
                return;
            }
        }

        public void RequestStop()
        {
            if (m_bServerLoop)
                m_bServerLoop = false;
            Thread.Sleep(100);

            if (m_pipeServer != null)
            {
                m_pipeServer.Close();
            }
        }

        public Boolean IsServerRunning()
        {
            return m_bServerLoop;
        }

    }
}