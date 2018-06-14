#!/bin/bash

# Check log directory
if [ ! -d "/var/log/azvmmanager" ]; then
        # Create Log directory
        mkdir /var/log/azvmmanager
fi

# set date
DATE=`date +%Y%m%d%H%M%S`

# Start logging
exec 1>"/var/log/azvmmanager/azvmmanager$DATE.log" 2>&1
echo "$(date) : ##################################################" >&1
echo "$(date) : ##################################################" >&1
echo "$(date) : ##################################################" >&1

echo "$(date) : # Set config file" >&1
# Set config file
configFile='./config/azvmmanager.cfg'

echo "$(date) : # Get variables from config" >&1
# Get variables from config
## Zone
vmName=$(jq -r '.vmName' "$configFile")
vmResourceGroup=$(jq -r '.vmResourceGroup' "$configFile")
## Azure
cloudName=$(jq -r '.azure.cloudName' "$configFile")
clientID=$(jq -r '.azure.clientID' "$configFile")
clientSecret=$(jq -r '.azure.clientSecret' "$configFile")
tenant=$(jq -r '.azure.tenant' "$configFile")
subscriptionID=$(jq -r '.azure.subscriptionID' "$configFile")

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

                # Start VM
                echo "$(date) : # Start VM: $vmName" >&1
                az vm start --name $vmName --resource-group $vmResourceGroup --verbose
                if [ $? = 0 ];
                then
                        echo "$(date) : # $vmName is up and running" >&1
                else
                        echo "$(date) : # Something went wrong during $vmName starting." >&1
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
