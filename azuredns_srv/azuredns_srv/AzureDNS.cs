using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Runtime.InteropServices;

// Internal elements
using Enum;

namespace azuredns_srv
{
    public partial class AzureDNS : ServiceBase
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        public AzureDNS()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("In OnStart");

            // Get values from Configuration files
            //// updateIntervalInMin
            string updateIntervalInMin =
                System.Configuration.ConfigurationManager.AppSettings.Get("updateIntervalInMin");

            //// Dns zone name
            string zoneName =
                System.Configuration.ConfigurationManager.AppSettings.Get("zoneName");

            //// A record name
            string aRecordName =
                System.Configuration.ConfigurationManager.AppSettings.Get("aRecordName");

            //// dnsResourceGroup
            string dnsResourceGroup =
                System.Configuration.ConfigurationManager.AppSettings.Get("dnsResourceGroup");

            //// cloudName - AzureCloud, AzureGermanCloud
            string cloudName =
                System.Configuration.ConfigurationManager.AppSettings.Get("azure.cloudName");

            //// clientID
            string clientID =
                System.Configuration.ConfigurationManager.AppSettings.Get("azure.clientID");

            //// clientSecret
            string clientSecret =
                System.Configuration.ConfigurationManager.AppSettings.Get("azure.clientSecret");

            //// Tenant
            string tenant =
                System.Configuration.ConfigurationManager.AppSettings.Get("azure.tenant");

            //// subscriptionID
            string subscriptionID =
                System.Configuration.ConfigurationManager.AppSettings.Get("azure.subscriptionID");


            // Set up a timer to trigger updateIntervalInMin.  
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = Int32.Parse(updateIntervalInMin) * 1000;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            timer.Start();

        }

        private int eventId = 1;

        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            // TODO: Insert monitoring activities here.  
            eventLog1.WriteEntry("Monitoring the System", EventLogEntryType.Information, eventId++);
        }


    protected override void OnStop()
        {
            eventLog1.WriteEntry("In onStop.");
        }

        protected override void OnContinue()
        {
            eventLog1.WriteEntry("In OnContinue.");
        }

        private void eventLog1_EntryWritten(object sender, EntryWrittenEventArgs e)
        {

        }
    }
}
