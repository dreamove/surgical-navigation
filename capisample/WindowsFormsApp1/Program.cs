using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using Zeroconf;
using System.IO.Ports;
using NDI.CapiSample;
using NDI.CapiSample.Data;
using NDI.CapiSample.Protocol;


namespace WindowsFormsApp1
{
    class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
      
        [STAThread]

        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //log("C# CAPI Sample v" + Capi.GetVersion());
            //Capi cAPI;
            //string host = "COM3";
            //cAPI = new CapiSerial(host);

            //Run(cAPI);

            //Reciever(cAPI);
            Application.Run(new Form1());
           

        }
        //private void button1_Click(object sender, EventArgs e)
        //{

        //}
        public static void log(string message)
        {
            //string time = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            
            Console.WriteLine(message);
        }

       

        private static bool InitializePorts(Capi cAPI)
        {
            // Polaris Section
            // ---
            // Request a new tool port handle so that we can load an SROM
            Port tool = cAPI.PortHandleRequest();
            if (tool == null)
            {
                log("Could not get available port for tool.");
            }
            else if (!tool.LoadSROM("sroms/probe.rom"))
            {
                log("Could not load SROM file for tool.");
                return false;
            }
            // ---

            // Initialize all ports not currently initialized
            var ports = cAPI.PortHandleSearchRequest(PortHandleSearchType.NotInit);
            foreach (var port in ports)
            {
                if (!port.Initialize())
                {
                    log("Could not initialize port " + port.PortHandle + ".");
                    return false;
                }

                if (!port.Enable())
                {
                    log("Could not enable port " + port.PortHandle + ".");
                    return false;
                }
            }

            // List all enabled ports
            //    log("Enabled Ports:");
            //    ports = cAPI.PortHandleSearchRequest(PortHandleSearchType.Enabled);
            //    foreach (var port in ports)
            //    {
            //        port.GetInfo();
            //        log(port.ToString());
            //    }

            return true;
        }

        /// <summary>
        /// Check for BX2 command support.
        /// </summary>
        /// <param name="apiRevision">API revision string returned by CAPI.GetAPIRevision()</param>
        /// <returns>True if supported.</returns>
        //private static bool IsBX2Supported(string apiRevision)
        //{
        //    // Refer to the API guide for how to interpret the APIREV response
        //    char deviceFamily = apiRevision[0];
        //    int majorVersion = int.Parse(apiRevision.Substring(2, 3));

        //    // As of early 2017, the only NDI device supporting BX2 is the Vega
        //    // Vega is a Polaris device with API major version 003
        //    if (deviceFamily == 'G' && majorVersion >= 3)
        //    {
        //        return true;
        //    }

        //    return false;
        //}
        private static void Reciever(Capi cAPI)
        {
            for (int i = 0; i < 3; i++)
            {
                //if (!cAPI.IsConnected)
                //{
                //    log("Disconnected while tracking.");
                //    break;
                //}
                cAPI.SendBX();
                List<Tool> tools = cAPI.SendBX();
                foreach (var t in tools)
                {
                    log(t.ToString());
                }
            }
            if (!cAPI.TrackingStop())
            {
                log("Could not stop tracking.");
                return;
            }
            log("TrackingStopped");

            if (!cAPI.Disconnect())
            {
                log("Could not disconnect.");
                return;
            }
            log("Disconnected");
        }
        /// <summary>
        /// Run the CAPI sample regardless of the connection method.
        /// </summary>
        /// <param name="cAPI">The configured CAPI protocol.</param>
        private static void Run(Capi cAPI)
        {
            // Be Verbose
            // cAPI.LogTransit = true;

            // Use the same log output format as this sample application
          //  cAPI.SetLogger(log);

            if (!cAPI.Connect())
            {
                log("Could not connect to " + cAPI.GetConnectionInfo());
                //  PrintHelp();
                return;
            }
            log("Connected");

            // Get the API Revision this will tell us if BX2 is supported.
            //string revision = cAPI.GetAPIRevision();
            //log("Revision:" + revision);

            if (!cAPI.Initialize())
            {
                log("Could not initialize.");
                return;
            }
            log("Initialized");

            // The Frame Frequency may not be possible to set on all devices, so an error response is okay.
            //cAPI.SetUserParameter("Param.Tracking.Frame Frequency", "60");
            //cAPI.SetUserParameter("Param.Tracking.Track Frequency", "2");

            //// Read the final values
            //log(cAPI.GetUserParameter("Param.Tracking.Frame Frequency"));
            //log(cAPI.GetUserParameter("Param.Tracking.Track Frequency"));

            // Initialize tool ports
            if (!InitializePorts(cAPI))
            {
                return;
            }
            //log("初始化端口");
            if (!cAPI.TrackingStart())
            {
                log("Could not start tracking.");
                return;
            }
            log("TrackingStarted");

            // Track several frames of data using the BX command.
            //for (int i = 0; i < 10; i++)
            //{
            //    if (!cAPI.IsConnected)
            //    {
            //        log("Disconnected while tracking.");
            //        break;
            //    }

            //    List<Tool> tools = cAPI.SendBX();
            //    foreach (var t in tools)
            //    {
            //        log(t.ToString());
            //    }
            //}

            // Track several frames of data using the BX2 Command
            //if (IsBX2Supported(revision))
            //{
            //    for (int i = 0; i < 300; i++)
            //    {
            //        if (!cAPI.IsConnected)
            //        {
            //            log("Disconnected while tracking.");
            //            break;
            //        }

            //        List<Tool> tools = cAPI.SendBX2("--6d=tools");
            //        foreach (var t in tools)
            //        {
            //            log(t.ToString());
            //            //foreach (var m in t.markers)
            //            //{
            //            //    log(m.ToString());
            //            //}
            //        }
            //    }
            //}

            //if (!cAPI.TrackingStop())
            //{
            //    log("Could not stop tracking.");
            //    return;
            //}
            //log("TrackingStopped");

            //if (!cAPI.Disconnect())
            //{
            //    log("Could not disconnect.");
            //    return;
            //}
            //log("Disconnected");
        }
    }
}
