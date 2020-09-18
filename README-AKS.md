# Dapr Retro Point of Sales

## Script for AKS deployment setup

```bash
#!/bin/bash
clear

# NOTE: before run this script ensure you are logged in Azure by using az login command.

read -p "Introduce a lowercase unique alias for your deployment (max length suggested of 6 chars): " DeploymentAlias
ResourceGroupName=$DeploymentAlias"-aks-group"
AKSClusterName=$DeploymentAlias"-aks"
Location="westus2"
AKSK8sVersion="1.17.9"
ContainerRegistryName=$DeploymentAlias"cr01"
ServicePrincipal=$DeploymentAlias"-aks-sp"

# PRINT
echo "**********************************************************************"
echo " CREATING: SERVICE PRINCIPAL"
echo ""
echo " Description:"
echo ""
echo " An identity created for use with applications, hosted services, and "
echo " automated tools to access Azure resources."
echo "**********************************************************************"

az ad sp create-for-rbac -n $ServicePrincipal

# get app id
SP_APP_ID=$(az ad sp show --id http://$ServicePrincipal --query appId -o tsv)
echo "Service Principal AppId: "$SP_APP_ID

# get password
SP_APP_PASSWORD=$(az ad sp credential reset --name $ServicePrincipal --query password -o tsv)
echo "Service Principal Password: "$SP_APP_PASSWORD

# wait aad propagation
sleep 60s

# PRINT
echo "**********************************************************************"
echo " CREATING: GENERAL RESOURCE GROUP"
echo ""
echo " Description:"
echo ""
echo " A container that holds related resources for an Azure solution."
echo "**********************************************************************"

# create cluster resource group
az group create --name $ResourceGroupName --location $Location

# get group id
GROUP_ID=$(az group show -n $ResourceGroupName --query id -o tsv)

# role assignment 
az role assignment create --assignee $SP_APP_ID --scope $GROUP_ID --role "Contributor"

# PRINT
echo "**********************************************************************"
echo " CREATING: CONTAINER REGISTRY"
echo ""
echo " Description:"
echo ""
echo " A registry of Docker and Open Container Initiative (OCI) images, with"
echo " support for all OCI artifacts."
echo "**********************************************************************"

# create acr
az acr create -n $ContainerRegistryName -g $ResourceGroupName --sku basic --admin-enabled true

# get container registry id
ContainerRegistryId=$(az acr show -n $ContainerRegistryName -g $ResourceGroupName --query id -o tsv)

# get acr id 
ACR_ID=$(az acr show -n $ContainerRegistryName -g $ResourceGroupName --query id -o tsv)

# role assignment 
az role assignment create --assignee $SP_APP_ID --scope $ACR_ID --role "Contributor"

# PRINT
echo "**********************************************************************"
echo " CREATING: AKS CLUSTER"
echo ""
echo " Description:"
echo ""
echo " Highly available, secure, and fully managed Kubernetes service."
echo "**********************************************************************"

# create cluster
az aks create \
    --name $AKSClusterName \
    --resource-group $ResourceGroupName \
    --node-count 3 \
    --kubernetes-version $AKSK8sVersion \
    --service-principal $SP_APP_ID \
    --client-secret $SP_APP_PASSWORD \
    --generate-ssh-keys

# update cluster
az aks update \
    --resource-group $ResourceGroupName \
    --name $AKSClusterName \
    --attach-acr $ACR_ID

echo ""
echo "****************************CALL TO ACTION****************************"
echo ""
echo "Deployment alias: "$DeploymentAlias
echo "Resource group: "$ResourceGroupName
echo "Location: "$Location
echo "Cluster name: "$AKSClusterName
echo "Kubernetes version: "$AKSK8sVersion
echo "Container registry: "$ContainerRegistryName
echo "Service principal name: "$ServicePrincipal
echo "Service principal id: "$SP_APP_ID
echo "Service principal password: "$SP_APP_PASSWORD
echo ""
echo "****************************CALL TO ACTION****************************"
echo ""
```

## Script for AKS deployment removal

```bash
#!/bin/bash
clear

# NOTE: before run this script ensure you are logged in Azure by using az login command.

read -p "Introduce a lowercase unique alias of the deployment you want to cleanup: " DeploymentAlias
ResourceGroupName=$DeploymentAlias"-aks-group"

# get location
LOCATION=$(az group show -n $ResourceGroupName --query location -o tsv)
ResourceNodeGroupName="MC_"$ResourceGroupName"_"$DeploymentAlias"-aks_"$LOCATION

echo "Operation: Delete resource group "$ResourceGroupName 
# delete delete general resource group
az group delete -n $ResourceGroupName --no-wait

echo "Operation: Delete resource group "$ResourceNodeGroupName
# delete delete aks node group resource group
az group delete -n $ResourceNodeGroupName --no-wait

# get service principal
SP_APP_ID=$(az ad sp show --id http://$DeploymentAlias"-aks-sp" --query appId -o tsv)

# delete service principal
az ad sp delete --id $SP_APP_ID
```