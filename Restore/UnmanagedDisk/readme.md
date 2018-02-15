# Restore unmanaged disk VM

This tool helps you to restore a VM with unmanaged disks

## Required naming convention
OS disk and Data disks realted vhds must be in the following format:
* OS disk:
	<vm name>-osdisk.vhd
* Data disk:
	<vm name>-datadisk-<number of datadisk from 0>.vhd 
	(example for 1st data disk: myvm-datadisk-0.vhd)

## Prerequisites
* Azure-Cli 2.x
* Python 2.7

## Usage
0. Make a note with the name and size of the VM
1. Restore VMs VHD from backup vault
	> Please use it according to your documentation
2. Stop deallocated the VM
	> You can do it from Azure Portal, PowerShell and Azure-Cli 2.0
3. Login your linux computer
4. Download files from git
5. Navigate the unmanagedDisk directory
6. Run this command to delete VM object and the corrupt VHDs 
	```bash
	bash 01_deleteVMandVHDs.sh 
	```

	> Required: SubscriptionID, VM Name, ResourceGroup where VM is located, StorageAccount name, StorageAccount key (Put between " and " the value!)
7. Run this command to copy restored VHD files to the original location
	```bash
	bash 02_copyRestoredVHDs.sh
	```
	> Required: SubscriptionID, VM Name, StorageAccount name, StorageAccount key (Put between " and " the value!), Restored VHD container, Destination container for VHD (This is 'vhds' by system design documentation)
8. Deploy the VM with your ARM template from the Portal (or PowerShell and Azure-Cli 2.0)
	> Required: environmentUniquePrefix, virtualMachine_ID, os, worker_VMSize, number_Of_DataDisks, dataDisks_SizeGB
9. If deployment result is *Success* please check VM's status and consistency
10. Delete OLD_<VM name>....vhd from storage account 