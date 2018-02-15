#!/bin/bash

version="1.1"

# Get parameters for deletion
read -p 'SubscriptionID: ' SubscriptionID
read -p 'VM Name: ' deleteVM
read -p 'ResourceGroup where VM is located: ' rgVM
read -p 'AZURE_STORAGE_ACCOUNT: ' AZURE_STORAGE_ACCOUNT
read -p 'AZURE_STORAGE_ACCESS_KEY (Put between " and " the value!) : ' AZURE_STORAGE_ACCESS_KEY

# Logout Azure
echo "Logout from Azure"
az logout

# Login to Azure
## Checks active connections
oLoggedIn=$(az account show -o table --subscription $SubscriptionID | wc -l);
if [ $oLoggedIn -lt 1 ];
then
    echo "Login Azure..."
    # Get Azure cloud name
    read -p 'AzureEnvironment (eg: AzureGermanCloud): ' azureEnvironment
    # Set could name
    az cloud set --name $azureEnvironment;
    # Get Username for subscription
    IFS= read -p "Please type username for Azure Login:" userName;
    # Login to Azure
    az login -u $userName;
    # Get account list
    az account list --output table
    
    # Set subscription
    az account set --subscription $SubscriptionID;
fi
# Get account list
az account list --output table;
oAzureAccount=$(az account show);

# Delete VM
while true; do
    read -p "Do you wish to delete $deleteVM?" yn
    case $yn in
        [Yy]* ) break;;
        [Nn]* ) exit;;
        * ) echo "Please answer yes or no.";;
    esac
done
echo "- Start delete $deleteVM"
az vm delete -o table --name $deleteVM -g $rgVM  --yes ;

# Copy old VHDs
echo "- Make a copy from $deleteVM-osdisk.vhd. New name: OLD-$deleteVM-osdisk.vhd"
az storage blob copy start --account-name $AZURE_STORAGE_ACCOUNT --account-key $AZURE_STORAGE_ACCESS_KEY --destination-blob "OLD-$deleteVM-osdisk.vhd" --destination-container 'vhds' --source-account-name $AZURE_STORAGE_ACCOUNT --source-account-key $AZURE_STORAGE_ACCESS_KEY --source-container 'vhds' --source-blob "$deleteVM-osdisk.vhd";

for (( i=0; i<8; i++)); do
    echo "- Make a copy from $deleteVM-datadisk-$i.vhd. New name: OLD-$deleteVM-datadisk-$i.vhd";
    az storage blob copy start --account-name $AZURE_STORAGE_ACCOUNT --account-key $AZURE_STORAGE_ACCESS_KEY --destination-blob "OLD-$deleteVM-datadisk-$i.vhd" --destination-container 'vhds' --source-account-name $AZURE_STORAGE_ACCOUNT --source-account-key $AZURE_STORAGE_ACCESS_KEY --source-container 'vhds' --source-blob "$deleteVM-datadisk-$i.vhd";
done



# Delete VHD files
while true; do
    read -p "Do you wish to delete old VHD files for $deleteVM?" yn
    case $yn in
        [Yy]* ) break;;
        [Nn]* ) exit;;
        * ) echo "Please answer yes or no.";;
    esac
done
echo "- Delete $deleteVM-osdisk.vhd"
az storage blob delete -c vhds --account-key $AZURE_STORAGE_ACCESS_KEY --account-name $AZURE_STORAGE_ACCOUNT  -n "$deleteVM-osdisk.vhd";

for (( i=0; i<8; i++)); do
    echo "- Delete $deleteVM-datadisk-$i.vhd";
    az storage blob delete -c vhds --account-key $AZURE_STORAGE_ACCESS_KEY --account-name $AZURE_STORAGE_ACCOUNT -n "$deleteVM-datadisk-$i.vhd";
done
