Azure VM Manegr
===

# Background

Everybody knows the automatic VM shutdown function in Azure. This solution helps you to start your VM in Azure at that time when you schedule. So when you arrive to yout workplace your VM is up and running every time. :-)


# Prerequisites

* Linux OS (tested on CentOS 7)
* Azure-Cli 2.x
* Azure subscription

# Installation

1. Pull the solution from my git
2. Create a Service Principal for your account. [More information is here.](http://www.the1bit.hu/technical-thursday-azure-resources-with-ansible/#create-service-principal)
3. Edit configuration file in **config** directory
``` bash 
{
        "vmName": "<name of your vm>",
		"vmResourceGroup": "<vm resource group>",
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

### Configure to start your vm at 9AM every weekdays
0 9 * * mon,tue,wed,thu,fri root cd /root/scripts/azvmmanager;bash azvmmanager.sh;

```
6. Wait for the required time then check the logs according to execution in */var/log/azvmmanager* directory
``` bash
less /var/log/azvmmanager/azvmmanager20180614090001.log 
```
**part of the result:**
``` bash
...
Thu Jun 14 09:00:29 CEST 2018 : # Login success
Thu Jun 14 09:00:29 CEST 2018 : # Set default subscription
Thu Jun 14 09:00:38 CEST 2018 : # Default subscription has been set
Thu Jun 14 09:00:38 CEST 2018 : # Start VM: xxxxxxxxx
...
```

# Other

Feel free to get in touch if you ave any questions or you need some help.

