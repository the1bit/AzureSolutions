#!/bin/bash

# Check log directory
if [ ! -d "/var/log/azdns" ]; then
        # Create Log directory
        mkdir /var/log/azdns
fi

# set date
DATE=`date +%Y%m%d%H%M%S`

# Start logging
exec 1>"/var/log/azdns/azdns$DATE.log" 2>&1
echo "$(date) : ##################################################" >&1
echo "$(date) : #### azdns v18.6.0 - the1bit" >&1
echo "$(date) : ##################################################" >&1

echo "$(date) : # Set config file" >&1
# Set config file
configFile='./config/azdns.cfg'

echo "$(date) : # Get variables from config" >&1
# Get variables from config
## Zone
zoneName=$(jq -r '.zoneName' "$configFile")
aRecordName=$(jq -r '.aRecordName' "$configFile")
## Azure
cloudName=$(jq -r '.azure.cloudName' "$configFile")
clientID=$(jq -r '.azure.clientID' "$configFile")
clientSecret=$(jq -r '.azure.clientSecret' "$configFile")
tenant=$(jq -r '.azure.tenant' "$configFile")
subscriptionID=$(jq -r '.azure.subscriptionID' "$configFile")
dnsResourceGroup=$(jq -r '.dnsResourceGroup' "$configFile")

echo "$(date) : # Login to Azure" >&1
# Login to Azure
az cloud update
az cloud set -n $cloudName
az login --service-principal -u $clientID -p $clientSecret -t $tenant
if [ $? = 0 ];
then

        echo "$(date) : # Login success" >&1
        # Login success
        echo "$(date) : # Set default subscription" >&1
        # Set default subscription
        az account set  --subscription $subscriptionID
        if [ $? = 0 ];
        then
                echo "$(date) : # Default subscription has been set" >&1
                # Default subscription has been set
                echo "$(date) : # Get current Puplic IP from Internet" >&1
                # Get current Puplic IP from Internet
                pip=$(curl ipinfo.io/ip)
                if [ ${#pip} -gt 9 ]; then
                        currentPIP=$(az network dns record-set a show -g "$dnsResourceGroup" -z "$zoneName" -n "$aRecordName" --query "arecords[0].ipv4Address" | cut -d '"' -f 2)
                        if [[ $pip != $currentPIP ]];
                        then
                                        echo "Current IP in Azure $currentPIP is different than my PIP: $pip. I update Azure now."
                                        az network dns record-set a update -g "$dnsResourceGroup" -z "$zoneName" -n "$aRecordName" --set arecords[0].ipv4Address=$pip
                        else
                                        echo "Current IP in Azure $currentPIP is same than my PIP: $pip. Not necessary to update"
                        fi
                else
                        echo "Puplic IP is not valid"
                fi
        else
                echo "something went wrong during Subscription setting"
        fi
                echo "$(date) : # Logout from Azure" >&1
                # Logout from Azure
        az logout
else
        echo "something went wrong during Azure login"
fi
echo "END"
