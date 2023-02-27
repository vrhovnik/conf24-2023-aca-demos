# create resource group if it doesn't exist with bicep file stored in bicep folder
# az bicep build -f ./bicep/rg.bicep
az deployment sub create -f ./bicep/rg.bicep -p resourceGroupName="Conf24RG"