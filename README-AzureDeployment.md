# Azure Deployment

## Summary

1. [Azure resources deployment script](#azure-resources-deployment-script)
2. [Azure resources removal script](#azure-resources-removal-script)

## Azure resources deployment script

1. Connect to Azure Container Registry via Azure CLI.

    ```
    az login
    ```

2. Run the script: powershell/azure-deployment.ps1 to deploy the Azure resources.

    ```
    .\powershell\azure-deployment.ps1
    ```

    Note: At the end of the script execution there is a call to action to save the information that will be used in the future, be sure keep safe that information.

    ```
    ****************************CALL TO ACTION****************************

    Deployment alias: 
    Subscription id: 
    Tenant id: 
    Resource group: 
    Location: 
    Cluster name: 
    Kubernetes version: 
    Managed identity name: 
    Managed identity id: 
    Managed identity client id: 
    Container registry name: 
    Key vault name: 
    Storage name: 
    Storage connection string: 
    Storage primary key: 
    Service bus namespace name: 
    Service bus connection string: 
    CosmosDB account name: 
    CosmosDB database name: 
    CosmosDB workflow 1 container name: 
    CosmosDB primary key: 
    CosmosDB connection string: 

    ****************************CALL TO ACTION****************************
    ```

## Azure resources removal script

1. Run the script: powershell/azure-removal.ps1 to remove the Azure resources.
    
    ```
    .\powershell\azure-removal.ps1
    ```