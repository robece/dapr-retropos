# NOTE: before run this script ensure you are logged in Azure by using "az login" command.

$DeploymentAlias = Read-Host -Prompt "Introduce a lowercase unique alias for your deployment (max length suggested of 9 chars)"
$ResourceGroupName = "$($DeploymentAlias)-group"
$Location = "westus2"
$AKSClusterName = "$($DeploymentAlias)ks"
$AKSK8sVersion = "1.17.9"
$ContainerRegistryName = "$($DeploymentAlias)cr"
$KeyVaultAccountName = "$($DeploymentAlias)kv"
$ManagedIdentityName = "$($AKSClusterName)-agentpool"
$StorageAccountName = "$($DeploymentAlias)stg"
$ServiceBusNamespaceName = "$($DeploymentAlias)sbns"
$CosmosDBAccountName = "$($DeploymentAlias)cos"
$CosmosDBDatabaseName = "$($DeploymentAlias)db"
$CosmosDBContainerWorkflow1Name = "workflow1-consumer"

$SubscriptionId = $(az account show --query id -o tsv)
$TenantId = $(az account show --query tenantId -o tsv)

# PRINT
Write-Host "**********************************************************************"
Write-Host " CREATING: GENERAL RESOURCE GROUP"
Write-Host ""
Write-Host " Description:"
Write-Host ""
Write-Host " A container that holds related resources for an Azure solution."
Write-Host "**********************************************************************"

# create cluster resource group
az group create --name $ResourceGroupName --location $Location

# PRINT
Write-Host "**********************************************************************"
Write-Host " CREATING: CONTAINER REGISTRY"
Write-Host ""
Write-Host " Description:"
Write-Host ""
Write-Host " A registry of Docker and Open Container Initiative (OCI) images, with"
Write-Host " support for all OCI artifacts."
Write-Host "**********************************************************************"

# create acr
az acr create -n $ContainerRegistryName -g $ResourceGroupName --sku basic --admin-enabled true

# get acr id 
$ACR_ID = $(az acr show -n $ContainerRegistryName -g $ResourceGroupName --query id -o tsv)

# PRINT
Write-Host "**********************************************************************"
Write-Host " CREATING: KEY VAULT"
Write-Host ""
Write-Host " Description:"
Write-Host ""
Write-Host " Azure cloud service that provides a secure store for keys, passwords,"
Write-Host " certificates, and other secrets."
Write-Host "**********************************************************************"

# create key vault
az keyvault create -n $KeyVaultAccountName -g $ResourceGroupName --sku standard 

# get key vault id 
$KV_ID = $(az keyvault show -n $KeyVaultAccountName -g $ResourceGroupName --query id -o tsv)

# PRINT
Write-Host "**********************************************************************"
Write-Host " CREATING: KUBERNETES SERVICE"
Write-Host ""
Write-Host " Description:"
Write-Host ""
Write-Host " Highly available, secure, and fully managed Kubernetes service."
Write-Host "**********************************************************************"

# create cluster
az aks create --name $AKSClusterName `
    --resource-group $ResourceGroupName `
    --node-count 3 --kubernetes-version $AKSK8sVersion `
    --enable-managed-identity `
    --attach-acr $ACR_ID `
    --generate-ssh-keys

# PRINT
Write-Host "**********************************************************************"
Write-Host " GRANTING PERMISSIONS"
Write-Host "**********************************************************************"

# get identity id
$aks=$(az aks show -n $AKSClusterName -g $ResourceGroupName | ConvertFrom-Json)
$identity=$(az identity show -n $ManagedIdentityName -g $aks.nodeResourceGroup | ConvertFrom-Json)

az role assignment create --role "Reader" --assignee $identity.principalId --scope $KV_ID

# PRINT
Write-Host "**********************************************************************"
Write-Host " CREATING: STORAGE"
Write-Host ""
Write-Host " Description:"
Write-Host ""
Write-Host " Azure Storage includes object, file, disk, queue, and table storage."
Write-Host "**********************************************************************"

az storage account create -n $StorageAccountName -g $ResourceGroupName -l $Location --sku Standard_LRS
$StorageAccountConnectionString=$(az storage account show-connection-string -n $StorageAccountName -g $ResourceGroupName -o tsv)
$StorageAccountPrimaryKey=$(az storage account keys list -n $StorageAccountName -g $ResourceGroupName --query [0].value -o tsv)

# adding storage connection string secret
az keyvault secret set --name "storage-connectionstring-$($StorageAccountName)" --vault-name $KeyVaultAccountName --value $StorageAccountConnectionString

# adding storage primary key secret
az keyvault secret set --name "storage-primarykey-$($StorageAccountName)" --vault-name $KeyVaultAccountName --value $StorageAccountPrimaryKey

# PRINT
Write-Host "**********************************************************************"
Write-Host " CREATING: SERVICE BUS NAMESPACE"
Write-Host ""
Write-Host " Description:"
Write-Host ""
Write-Host " A namespace provides a scoping container for addressing Service Bus."
Write-Host "**********************************************************************"

az servicebus namespace create -n $ServiceBusNamespaceName -g $ResourceGroupName -l $Location --sku Premium
$ServiceBusPrimaryConnectionString=$(az servicebus namespace authorization-rule keys list -g $ResourceGroupName --namespace-name $ServiceBusNamespaceName --name RootManageSharedAccessKey --query primaryConnectionString --output tsv)

# adding service bus connection string secret
az keyvault secret set --name "servicebus-connectionstring-$($ServiceBusNamespaceName)" --vault-name $KeyVaultAccountName --value $ServiceBusPrimaryConnectionString

# PRINT
Write-Host "**********************************************************************"
Write-Host " CREATING: COSMOS DB"
Write-Host ""
Write-Host " Description:"
Write-Host ""
Write-Host " Azure Cosmos DB is a fully managed NoSQL database service for modern "
Write-Host " app development."
Write-Host "**********************************************************************"

# create cosmosdb account
az cosmosdb create -n $CosmosDBAccountName -g $ResourceGroupName --kind GlobalDocumentDB --default-consistency-level Strong

# create cosmosdb database
az cosmosdb sql database create -n $CosmosDBDatabaseName --account-name $CosmosDBAccountName -g $ResourceGroupName

# create cosmosdb container
az cosmosdb sql container create -g $ResourceGroupName -a $CosmosDBAccountName -d $CosmosDBDatabaseName -n $CosmosDBContainerWorkflow1Name --partition-key-path "/partitionKey" --throughput "700"

# migrate cosmosdb throughput to autoscale
az cosmosdb sql container throughput migrate -g $ResourceGroupName -a $CosmosDBAccountName -d $CosmosDBDatabaseName -n $CosmosDBContainerWorkflow1Name -t autoscale

# get cosmosdb primary key
$CosmosDBPrimaryKey=$(az cosmosdb keys list --type connection-strings -n $CosmosDBAccountName -g $ResourceGroupName --type keys --query "primaryMasterKey" -o tsv)

# get cosmosdb connection string
$CosmosDBConnectionString=$(az cosmosdb keys list --type connection-strings -n $CosmosDBAccountName -g $ResourceGroupName --query "[connectionStrings][0][0].connectionString" -o tsv)

# adding cosmosdb primary key secret
az keyvault secret set --name "cosmosdb-primarykey-$($CosmosDBAccountName)" --vault-name $KeyVaultAccountName --value $CosmosDBPrimaryKey

# adding cosmosdb connection string secret
az keyvault secret set --name "cosmosdb-connectionstring-$($CosmosDBAccountName)" --vault-name $KeyVaultAccountName --value $CosmosDBConnectionString

Write-Host ""
Write-Host "****************************CALL TO ACTION****************************"
Write-Host ""
Write-Host " Deployment alias: $($DeploymentAlias)"
Write-Host " Subscription id: $($SubscriptionId)"
Write-Host " Tenant id: $($TenantId)"
Write-Host " Resource group: $($ResourceGroupName)"
Write-Host " Location: $($Location)"
Write-Host " Cluster name: $($AKSClusterName)"
Write-Host " Kubernetes version: $($AKSK8sVersion)"
Write-Host " Managed identity name: $($ManagedIdentityName)"
Write-Host " Managed identity id: $($identity.id)"
Write-Host " Managed identity client id: $($identity.clientId)"
Write-Host " Container registry name: $($ContainerRegistryName)"
Write-Host " Key vault name: $($KeyVaultAccountName)"
Write-Host " Storage name: $($StorageAccountName)"
Write-Host " Storage connection string: $($StorageAccountConnectionString)"
Write-Host " Storage primary key: $($StorageAccountPrimaryKey)"
Write-Host " Service bus namespace name: $($ServiceBusNamespaceName)"
Write-Host " Service bus connection string: $($ServiceBusPrimaryConnectionString)"
Write-Host " CosmosDB account name: $($CosmosDBAccountName)"
Write-Host " CosmosDB database name: $($CosmosDBDatabaseName)"
Write-Host " CosmosDB workflow 1 container name: $($CosmosDBContainerWorkflow1Name)"
Write-Host " CosmosDB primary key: $($CosmosDBPrimaryKey)"
Write-Host " CosmosDB connection string: $($CosmosDBConnectionString)"
Write-Host ""
Write-Host "****************************CALL TO ACTION****************************"
Write-Host ""