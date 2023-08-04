using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Management;
using System.IO;

namespace ApkInstaller
{
    class AdbPortCheckUtilites
    {
        public bool CheckIfExistAdbPortUsingProcesses()
        {
            int adbport = 5037;
            List<Port> nowPorts = new List<Port>();
            nowPorts = GetNetStatPorts(adbport);
            foreach (Port portItem in nowPorts)
            {
                if (Convert.ToInt32(portItem.port_number) == adbport)           
                {
                    //if (portItem.process_status.Equals("ESTABLISHED", StringComparison.Ordinal))
                    if (portItem.process_id != 0)
                    {
                        if (portItem.process_name.Equals("aiadb.exe", StringComparison.Ordinal))
                        {
                            try
                            {
                                Process[] proc_adb = Process.GetProcessesByName("aiadb");
                                foreach (var item in proc_adb)
                                {
                                    item.Kill();
                                }
                            }
                            catch (System.Exception ex)
                            {
                                return false;
                            }
                            return false;
                        }
                        portItem.process_path = LookupProcessPath(portItem.process_id);
                        Program.otherHelperProcessPath = portItem.process_path;
                        Program.otherHelperProcessID = portItem.process_id;
                        return true;
                    }                    
                }
            }
            return false;
        }

        public static List<Port> GetNetStatPorts(int portnum)
        {
            var Ports = new List<Port>();

            try
            {
                using (Process p = new Process())
                {
                    ProcessStartInfo ps = new ProcessStartInfo();
                    ps.Arguments = "-n -o -a";
                    ps.FileName = "netstat.exe";
                    ps.UseShellExecute = false;
                    ps.WindowStyle = ProcessWindowStyle.Hidden;
                    ps.CreateNoWindow = true;
                    ps.RedirectStandardInput = true;
                        ps.RedirectStandardOutput = true;
                    ps.RedirectStandardError = true;

                    p.StartInfo = ps;
                    p.Start();

                    StreamReader stdOutput = p.StandardOutput;
                    StreamReader stdError = p.StandardError;

                    string content = stdOutput.ReadToEnd() + stdError.ReadToEnd();
                    string exitStatus = p.ExitCode.ToString();

                    if (exitStatus != "0")
                    {
                        // Command Errored. Handle Here If Need Be
                    }

                    //Get The Rows
                    string[] rows = Regex.Split(content, "\r\n");
                    foreach (string row in rows)
                    {
                        //Split it baby
                        string[] tokens = Regex.Split(row, "\\s+");
                        if (tokens.Length > 4 && (tokens[1].Equals("TCP")))
                        {
                            string localAddress = Regex.Replace(tokens[2], @"\[(.*?)\]", "1.1.1.1");
                            Ports.Add(new Port
                            {
                                protocol = localAddress.Contains("1.1.1.1") ? String.Format("{0}v6", tokens[1]) : String.Format("{0}v4", tokens[1]),
                                port_number = localAddress.Split(':')[1],
                                process_id = Convert.ToInt32(tokens[5]),
                                process_name = tokens[1] == "UDP" ? LookupProcess(Convert.ToInt32(tokens[4])) : LookupProcess(Convert.ToInt32(tokens[5])),
                                process_status = tokens[4],
                                //process_path = LookupProcessPath(Convert.ToInt16(tokens[5]))
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Ports;
        }

        public static string LookupProcess(int pid)
        {
            string procName;
            try { procName = Process.GetProcessById(pid).ProcessName; }
            catch (Exception) { procName = "-"; }
            return procName;
        }

        public static string LookupProcessPath(int pid)
        {
            try
            {
                return Process.GetProcessById(pid).MainModule.FileName;
            }
            catch
            {
                string query = "SELECT ExecutablePath, ProcessID FROM Win32_Process";
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

                foreach (ManagementObject item in searcher.Get())
                {
                    object id = item["ProcessID"];
                    object path = item["ExecutablePath"];

                    if (path != null && id.ToString() == pid.ToString())
                    {
                        return path.ToString();
                    }
                }
            }

            return "";
        }
    }
}
