#!/usr/bin/python

""" Module: azuremgmt.restore
    Function collection for Restore VMs
	
	Version: 1.1
	Last modified date: 14.02.2018.
"""
import json
import subprocess

# FUNCTION Get OSD Disk name from backup file
def getOSDiskVHDName(filePath):
	# Open input file
	with open(filePath) as f:
		s = f.read()
	
	# Load resources from input file
	resourcesCount = len(json.loads(s)["resources"])
	# Set indicator to -1
	resourceIndex = -1
	# Seek on resources in input file
	for r in range(resourcesCount):
		# Work on virtualMachines types only
		if "virtualMachines" in json.loads(s)["resources"][r]["type"]:
			# Set resource index to current index
			resourceIndex = r
			# Get vhd uri
			uri = json.loads(s)["resources"][resourceIndex]["properties"]["storageProfile"]["osDisk"]["vhd"]["uri"]
			# Get VHD name
			vhdName = uri.split("/")[len(uri.split("/")) - 1]
			
			# Check vhd name length
			if len(vhdName) > 0:
				# It is not empty
				## Return with result
				print vhdName
				return vhdName
			else:
				# It is empty
				## Return with error
				print "error"
				return "error"

# FUNCTION Get Data Disk name from backup files
def getDataDiskVHDName(filePath):
	# Open input file
	with open(filePath) as f:
		s = f.read()
	# Load resources from input file
	resourcesCount = len(json.loads(s)["resources"])
	# Set indicator to -1
	resourceIndex = -1
	# Seek on resources in input file
	for r in range(resourcesCount):
		# Work on virtualMachines types only
		if "virtualMachines" in json.loads(s)["resources"][r]["type"]:
			# Set resource index to current index
			resourceIndex = r
			# Set vhdname to empty string
			vhdName = ""
			# Get datadisk count from file
			diskCount = len(json.loads(s)["resources"][resourceIndex]["properties"]["storageProfile"]["dataDisks"])
			# Seek on datadisks part
			for i in range(diskCount):
				# Get current disk uri
				uri = json.loads(s)["resources"][resourceIndex]["properties"]["storageProfile"]["dataDisks"][i]["vhd"]["uri"]
				# Get current disk name
				name = uri.split("/")[len(uri.split("/")) - 1]
				# Append the current disk name to vhdname list
				vhdName = vhdName + name + ";"
			
			# Check vhd name length
			if len(vhdName) > diskCount:
				# It is not empty
				## Return with result
				print vhdName
				return vhdName
			else:
				# It is empty
				## Return with error
				print "error"
				return "error"

# FUNCTION gives back storage uri according to Azure cloud name
def storageUriFromCloud(cloudName = "AzureGermanCloud"):
	# Check whether this is MCD
	if cloudName == "AzureGermanCloud":
		# If MCD it gives back the right uri
		print "blob.core.cloudapi.de"
		return "blob.core.cloudapi.de"
	else:
		# If MCI it gives back the right uri
		print "blob.core.windows.net"
		return "blob.core.windows.net"

# FUNCTION get datadisk information for required lun
def setDataDisk(filename, lun, VHDName, storageAccountName, cloudName = "AzureGermanCloud"):
	# Concatenate vhd uri
	vhdStorageUri = "https://" + storageAccountName + "." + storageUriFromCloud(cloudName) + "/vhds/" + VHDName + ".vhd"
	# Open file for read
	with open(filename, "r") as jsonFile:
		data = json.load(jsonFile)
	# Get resources count    
	resourcesCount = len(data["resources"])
	# Set resource index
	resourceIndex = -1
	# Seek on resource list (file)
	for r in range(resourcesCount):
		# It manages only virtualMachines objetc
		if "virtualMachines" in data["resources"][r]["type"]:
			# Set resource index value for the current index
			resourceIndex = r
			   
	# Set disk count from file
	diskCount = len(json.loads(s)["resources"][resourceIndex]["properties"]["storageProfile"]["dataDisks"])
	# Seek on data disk count
	for i in range(diskCount):
		# Manages the data which related the required lun
		if  int(data["resources"][resourceIndex]["properties"]["storageProfile"]["dataDisks"][i]["lun"]) == int(lun):
			# Put old value to a temp variable
			tmpName = data["resources"][resourceIndex]["properties"]["storageProfile"]["dataDisks"][i]["name"]
			# Set new value
			data["resources"][resourceIndex]["properties"]["storageProfile"]["dataDisks"][i]["name"] = VHDName

			# Put old uri to temp variable
			tmpUri = data["resources"][resourceIndex]["properties"]["storageProfile"]["dataDisks"][i]["vhd"]["uri"]
			# Set new value
			data["resources"][resourceIndex]["properties"]["storageProfile"]["dataDisks"][i]["vhd"]["uri"] = vhdStorageUri

	# Write back the changed content, if found in the file
	with open(filename, 'w') as jsonFile:
		json.dump(data, jsonFile)

# FUNCTION get osdisk information
def setOSDisk(filename, VHDName, storageAccountName, cloudName = "AzureGermanCloud"):
	# Concatenate vhd uri
	vhdStorageUri = "https://" + storageAccountName + "." + storageUriFromCloud(cloudName) + "/vhds/" + VHDName + ".vhd"
	# Open file for read
	with open(filename, "r") as jsonFile:
		data = json.load(jsonFile)
	# Get resources count     
	resourcesCount = len(data["resources"])
	# Set resource index
	resourceIndex = -1
	# Seek on resource list (file)
	for r in range(resourcesCount):
		# It manages only virtualMachines objetc
		if "virtualMachines" in data["resources"][r]["type"]:
			# Set resource index value for the current index
			resourceIndex = r
		
	# Set os disk uri by resource index from file
	uri = json.loads(s)["resources"][resourceIndex]["properties"]["storageProfile"]["osDisk"]["vhd"]["uri"]
	# Put old name ro temp variable  
	tmpName = data["resources"][resourceIndex]["properties"]["storageProfile"]["osDisk"]["name"]
	# Set new value
	data["resources"][resourceIndex]["properties"]["storageProfile"]["osDisk"]["name"] = VHDName

	# Put old uri to temp variable
	tmpUri = data["resources"][resourceIndex]["properties"]["storageProfile"]["osDisk"]["vhd"]["uri"]
	# Set new value
	data["resources"][resourceIndex]["properties"]["storageProfile"]["osDisk"]["vhd"]["uri"] = vhdStorageUri


	# Write back the changed content, if found in the file
	with open(filename, 'w') as jsonFile:
		json.dump(data, jsonFile)


# Get azuredeploy filename
def getFileName(command):
	# Execute command
	result = subprocess.check_output(command, shell=True)
	# Try to format in JSON
	try:
		# If success it give back as result
		print json.loads(result)[0]["name"]
		return json.loads(result)[0]["name"]
	except:
		# If it is not valid json result it gives back an error
		print "error"
		return "error"
