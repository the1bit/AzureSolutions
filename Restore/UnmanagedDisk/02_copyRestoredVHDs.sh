#!/bin/bash

version="1.0"

read -p 'SubscriptionID: ' SubscriptionID
read -p 'VM Name: ' vmName
read -p 'AZURE_STORAGE_ACCOUNT: ' AZURE_STORAGE_ACCOUNT
read -p 'AZURE_STORAGE_ACCESS_KEY (Put between " and " the value!) : ' AZURE_STORAGE_ACCESS_KEY
read -p 'Restored VHD container: ' sourceContainer
read -p 'Destination container for VHD: ' destinationContainer


oLoggedIn=$(az account show -o table --subscription $SubscriptionID | wc -l);
if [ $oLoggedIn -lt 1 ];
then
        echo "Login Azure..."
		read -p 'AzureEnvironment (eg: AzureGermanCloud): ' AzureEnvironment
        az cloud set --name $AzureEnvironment;
        IFS= read -p "Please type username for Azure Login:" userName;
        az login -u $userName;
		az account list --output table
		
        az account set --subscription $SubscriptionID;
fi
az account list --output table;
oAzureAccount=$(az account show);

echo "- Get azuredeploy file from $sourceContainer restore."
command="az storage blob list -c $sourceContainer --account-name $AZURE_STORAGE_ACCOUNT  --account-key $AZURE_STORAGE_ACCESS_KEY  --prefix azuredeploy"
azureFile=$(python -c "from restore import *; getFileName('$command')")

echo "- Download $azureFile file from $sourceContainer restore. Local name: azuredeploy.json"
az storage blob download -c $sourceContainer --account-name $AZURE_STORAGE_ACCOUNT  --account-key $AZURE_STORAGE_ACCESS_KEY -n $azureFile -f azuredeploy.json

echo "Copy restored OSdisk to the right place. Container: vhds, VHD: $vmName-osdisk.vhd"
osDisk=$(python -c "from restore import *; getOSDiskVHDName('azuredeploy.json')")
osDiskLen=${#osDisk}
if [ $osDiskLen -gt 5 ]; then
    az storage blob copy start --account-name $AZURE_STORAGE_ACCOUNT --account-key $AZURE_STORAGE_ACCESS_KEY --destination-blob "$vmName-osdisk.vhd" --destination-container $destinationContainer --source-account-name $AZURE_STORAGE_ACCOUNT --source-account-key $AZURE_STORAGE_ACCESS_KEY --source-container $sourceContainer --source-blob $osDisk;    
else
    echo "ERROR! No osdisk found for this VM"
    exit;
fi


dataDisks=$(python -c "from restore import *; getDataDiskVHDName('azuredeploy.json')")
dataDiskLen=${#dataDisks}
if [ $dataDiskLen -gt 5 ]; then
    diskNames=$(echo $dataDisks | tr ";" "\n")
    lunID=0
    for disk in $diskNames
    do
        echo $lunID;
        echo "Copy restored Data disks to the right place. Container: vhds, VHD: $vmName-datadisk-$lunID.vhd"
        az storage blob copy start --account-name $AZURE_STORAGE_ACCOUNT --account-key $AZURE_STORAGE_ACCESS_KEY --destination-blob "$vmName-datadisk-$lunID.vhd" --destination-container $destinationContainer --source-account-name $AZURE_STORAGE_ACCOUNT --source-account-key $AZURE_STORAGE_ACCESS_KEY --source-container $sourceContainer --source-blob $disk;
        ((lunID++));
    done
else
    echo "No datadisk found for this VM"
fi