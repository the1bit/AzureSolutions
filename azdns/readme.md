Dynamic DNS in Azure
===

# Background

This solution helps you to update your home server public IP dynamically. This is an alternative for NO-IP (where you have to confirm your host every 30 days). Here you don't need to confirm your host settings in a central surface.

This is not 100% free. The monthly cost in case of a "Pay-AS-YOU-GO" subscription is about **1 EUR/month**.

# Changelog

## v18.6.0

* Can update dns zone in Azure
* Manual **Crontab** configuration is required


# Prerequisites

* Linux OS (tested on CentOS 7)
* [Azure-Cli 2.x](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest)
* **[jq](https://stedolan.github.io/jq/download/)** package
* Azure subscription
* [Azure DNS zone](https://docs.microsoft.com/en-us/azure/dns/dns-zones-records)

# Installation

1. Pull the solution from my git
2. Create a Service Principal for your account. [More information is here.](http://www.the1bit.hu/technical-thursday-azure-resources-with-ansible/#create-service-principal)
3. Edit configuration file in **config** directory
``` bash 
{
	"zoneName": "<domain name>",
	"aRecordName": "<subdomain>",
	"dnsResourceGroup": "<DNS Zone resource group>",
	"azure": {
		"cloudName": "AzureCloud",
		"clientID": "<Service Principal ID>",
		"clientSecret": "<Service Principal Secret>",
		"tenant": "<Tenant ID>",
		"subscriptionID": "<Subscription ID>"
	}
}
```
4. Save configuration file
5. Configure crontab according to your update requirement
``` bash
# Edit crontab settings
vim /etc/crontab

### Configure to execute at 7AM and 7PM every day
0 7 * * * root cd /root/scripts/azdns;bash azdns.sh;
0 19 * * * root cd /root/scripts/azdns;bash azdns.sh;

```
6. Wait for the required time then check the logs according to execution in */var/log/azdns* directory
``` bash
less /var/log/azdns/azdns20180614070001.log 
```
**part of the result:**
``` bash
...
Thu Jun 14 07:00:29 CEST 2018 : # Login success
Thu Jun 14 07:00:29 CEST 2018 : # Set default subscription
Thu Jun 14 07:00:38 CEST 2018 : # Default subscription has been set
Thu Jun 14 07:00:38 CEST 2018 : # Get current Puplic IP from Internet
...
```

# Other

Feel free to get in touch if you ave any questions or you need some help.

