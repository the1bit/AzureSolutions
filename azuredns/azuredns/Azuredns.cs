using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// Azure related dependencies
using Microsoft.Rest.Azure.Authentication;
using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Azure.Management.Dns;
using Microsoft.Azure.Management.Dns.Models;

// My classes
using Enum;

using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;

namespace azuredns
{
    class Azuredns
    {
        private const string EVENTLOG_SOURCE = "azuredns";

        static void Main(string[] args)
        {
            //Create EventLog source
            if (!EventLog.SourceExists(EVENTLOG_SOURCE))
            {
                // Create source if does not exist
                EventLog.CreateEventSource(EVENTLOG_SOURCE, "Application");
                return;
            }

            // Init eventlog Writer
            EventLog azureLog = new EventLog();
            azureLog.Source = EVENTLOG_SOURCE;

            // Get values from Configuration files
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


            // Manage DNS
            ManageDNS(cloudName, tenant, clientID, clientSecret, subscriptionID, zoneName, aRecordName, Records.A , dnsResourceGroup, Operations.update, azureLog).Wait();

            azureLog.WriteEntry("azureDNS exit.", EventLogEntryType.Information, 299);
        }

        private static string GetPIP()
        {
            string externalIP = "";
            externalIP = (new WebClient()).DownloadString("http://ipinfo.io/ip");
            externalIP = (new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")).Matches(externalIP)[0].ToString();
            return externalIP;
        }

        private static async Task ManageDNS(string cloudName, string tenantId, string clientId, string secret, string subscriptionId, string zoneName, string recordName, Records record, string dnsResourceGroup, Operations Operation, EventLog azureLog)
        {

            // Put to log the status
            azureLog.WriteEntry("Start to login to Azure.\n---------------------\nTenant: " + tenantId + "\nClientID: " + clientId + "\nSubscriptionID: " + subscriptionId, EventLogEntryType.Information, 100);
            // Build the service credentials and DNS management client
            // Init dnsClient outside of try
            DnsManagementClient dnsClient = null;
            //object dnsClient = null;
            try
            {
                var serviceCreds = await ApplicationTokenProvider.LoginSilentAsync(tenantId, clientId, secret);
                dnsClient = new DnsManagementClient(serviceCreds);
                dnsClient.SubscriptionId = subscriptionId;
            }
                catch (System.Exception e)
            {
                azureLog.WriteEntry("Login error!\n---------------------\n" + e.Message + "\n---------------------\nProgram terminates and exit.", EventLogEntryType.Error, 400);
                // Exit
                return;
            }

            azureLog.WriteEntry("Login has been success to Azure.\n---------------------\nTenant: " + tenantId + "\nClientID: " + clientId + "\nSubscriptionID: " + subscriptionId, EventLogEntryType.Information, 101);

            // Try to create DNS zone
            #region Create DNS Zone
            // **********************************************************************************************************
            // Create DNS Zone
            // **********************************************************************************************************
            // Init dnsZone
            object dnsZone = null;
            // Init status and error messages
            var status = Status.unknown;
            Exception getMessage = null;
            Exception createMessage = null;
            // Check if dnsZone exists or not
            try
            {
                // get DNS zone
                dnsZone = dnsClient.Zones.Get(dnsResourceGroup, zoneName);
                // Set status if success
                status = Status.exists;
            }
            catch (System.Exception e)
            {
                // Set status to not exist
                status = Status.notexist;
                getMessage = e;
            }

            //Create dnsZone if does not exist
            if(status == Status.notexist)
            {
                try { 
                    // Create zone parameter
                    var dnsZoneParams = new Zone("global"); // All DNS zones must have location = "global"

                    // Create the actual zone.
                    // Note: Uses 'If-None-Match *' ETAG check, so will fail if the zone exists already.
                    // Note: For non-async usage, call dnsClient.Zones.CreateOrUpdate(resourceGroupName, zoneName, dnsZoneParams, null, "*")
                    // Note: For getting the http response, call dnsClient.Zones.CreateOrUpdateWithHttpMessagesAsync(resourceGroupName, zoneName, dnsZoneParams, null, "*")
                    dnsZone = await dnsClient.Zones.CreateOrUpdateAsync(dnsResourceGroup, zoneName, dnsZoneParams, null, "*");
                    // Set status
                    status = Status.success;
                }
                catch (System.Exception e)
                {
                    // Set status
                    status = Status.error;
                    createMessage = e;
                }
            }

            // Check dnsZone status and send message
            switch (status)
            {
                case Status.success:
                    azureLog.WriteEntry(
                        "DNS zone has been created successfully.\n---------------------\nzoneName: " + zoneName, 
                        EventLogEntryType.Information, 200);
                    break;
                case Status.error:
                    azureLog.WriteEntry(
                        "An error has been occurred during DNS zone creation.\n---------------------\nzoneName: " + zoneName + "\n" + createMessage.Message, 
                        EventLogEntryType.Error, 401);
                    // Exit from code
                    return;
                case Status.exists:
                    azureLog.WriteEntry(
                        "DNS zone exists.\n---------------------\nzoneName: " + zoneName, 
                        EventLogEntryType.Information, 202);
                    break;
            }
            #endregion

            // Get PIP
            string pip = GetPIP();

            if (pip.Length < 1)
            {
                // Exit with error
                azureLog.WriteEntry(
                    "An error has been occurred during public IP reading.", 
                    EventLogEntryType.Error, 403);
                return;
            }

            // Check record type
            switch (record)
            {
                case Records.A:

                    // Init existing variable
                    bool isExisting = false;

                    #region Update A Record
                    // **********************************************************************************************************
                    // Update A Record
                    // **********************************************************************************************************
                    try
                    {
                        var recordSet = dnsClient.RecordSets.Get(dnsResourceGroup, zoneName, recordName, RecordType.A);

                        // Get current PIP in Azure
                        string aPIP = recordSet.ARecords[0].Ipv4Address.ToString();
                        // Check whether we have to update the PIP or not
                        if (aPIP != pip)
                        {
                            // Add a new record to the local object.  Note that records in a record set must be unique/distinct
                            recordSet.ARecords.Clear();
                            recordSet.ARecords.Add(new ARecord(pip));

                            // Update the record set in Azure DNS
                            // Note: ETAG check specified, update will be rejected if the record set has changed in the meantime
                            recordSet = await dnsClient.RecordSets.CreateOrUpdateAsync(dnsResourceGroup, zoneName, recordName, RecordType.A, recordSet, recordSet.Etag);

                            // Write success event to log
                            azureLog.WriteEntry(
                                "Public IP update was success.\n---------------------\nzoneName: " + zoneName + "\nrecordName: " + recordName + "\nCurrent PIP: " + pip + "\nReplaced PIP in Azure: " + aPIP, 
                                EventLogEntryType.Information, 203);
                        }
                        else
                        {
                            // Write existing event to log
                            azureLog.WriteEntry(
                                "Not required to update Public IP.\n---------------------\nzoneName: " + zoneName + "\nrecordName: " + recordName + "\nCurrent PIP: " + pip + "\nPIP in Azure: " + aPIP, 
                                EventLogEntryType.Information, 204);
                        }
                        // Set existing parameter
                        isExisting = true;
                    }
                    catch (System.Exception e)
                    {
                        azureLog.WriteEntry(
                            "An error has been occurred during PIP update.\n---------------------\nzoneName: " + zoneName + "\nrecordName: " + recordName + "\nPIP: " + pip + "\n" + e.Message, 
                            EventLogEntryType.Error, 404);
                        return;
                    }
                    #endregion

                    if (! isExisting)
                    { 
                        #region Create A Record
                        // **********************************************************************************************************
                        // Create A Record
                        // **********************************************************************************************************
                        try
                        {
                            // Create record set parameters
                            var recordSetParams = new RecordSet();
                            recordSetParams.TTL = 30;

                            // Add records to the record set parameter object.  In this case, we'll add a record of type 'A'
                            recordSetParams.ARecords = new List<ARecord>();
                            recordSetParams.ARecords.Add(new ARecord(pip));

                            // Add metadata to the record set.  Similar to Azure Resource Manager tags, this is optional and you can add multiple metadata name/value pairs
                            recordSetParams.Metadata = new Dictionary<string, string>();
                            recordSetParams.Metadata.Add("host", recordName + '.' + zoneName);

                            // Create the actual record set in Azure DNS
                            // Note: no ETAG checks specified, will overwrite existing record set if one exists
                            var recordSet = await dnsClient.RecordSets.CreateOrUpdateAsync(dnsResourceGroup, zoneName, recordName, RecordType.A, recordSetParams);

                            // Write success event to log
                            azureLog.WriteEntry(
                                "Public IP has been created successfully.\n---------------------\nzoneName: " + zoneName + "\nrecordName: " + recordName + "\nCurrent PIP: " + pip, 
                                EventLogEntryType.Information, 205);
                        }
                        catch (System.Exception e)
                        {
                            azureLog.WriteEntry(
                                "An error has been occurred during PIP creation.\n---------------------\nzoneName: " + zoneName + "\nrecordName: " + recordName + "\nPIP: " + pip + "\n" + e.Message, 
                                EventLogEntryType.Error, 405);
                            return;
                        }
                        #endregion
                    }
                    break;
                case Records.AAA:
                    break;
                case Records.CNAME:
                    break;
                case Records.MX:
                    break;
                case Records.NS:
                    break;
                case Records.PTR:
                    break;
                case Records.SRV:
                    break;
                case Records.TXT:
                    break;
                default:
                    break;
            }
        }
    }
}
