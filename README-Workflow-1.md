# Workflow 1

## Summary

1. Description
2. Requirements
3. Code projects
4. Components required
5. Introductory documentation
6. Microservices port configuration for debugging
7. Applications to Container Registry
8. Kubernetes resources

## Description

The purpose of this workflow it's to represent an exposed API that will be receiving messages from an external worker service, these messages will be stored in the Azure Service Bus Queue and eventually they will be consumed and processed by a ServiceBusTrigger Function to finally send the results to an storage.

<div>
    <img src="resources/images/architecture-workflow-1.png" width="800" />
</div>

In this scenario, some KEDA components were added, to horizontal autoscale the pods based on the amount of events received in the queue, this behavior will be only available in Kubernetes.

Note: During the development of the project, I found some early adoption considerations, the first one, it's the lack of the support of Dapr queue trigger for Service Bus Queues, this forced to me to use the ServiceBusTrigger Function instead of a possible DaprQueueTrigger, this creates a direct dependency to the Service Bus resource, but once we have the Dapr queue trigger available for Functions, the resource will be fully adaptive to other non-Azure components, for further details about the Dapr Functions Extension project: https://github.com/dapr/azure-functions-extension.

## Code projects

- source/workflow-1/RetroPOS.ExposedService.Api
- source/workflow-1/RetroPOS.ExposedService.WorkerService
- source/workflow-1/RetroPOS.Consumer.Api

## Components required

- Output Binding (Dapr) - Azure Service Bus
- State (Dapr) - Redis, CosmosDB or any other supported component by Dapr
- Distributed Tracing (Dapr) - Zipkin, App Insights or any other supported component by Dapr

## Introductory documentation

- [Dapr overview](https://github.com/dapr/docs/tree/master/overview)
- [Dapr building blocks](https://github.com/dapr/docs/tree/master/concepts#building-blocks)
- [Setup Dapr development environment](https://github.com/dapr/dapr/blob/master/docs/development/setup-dapr-development-env.md)
- [Launch dapr and your app](https://github.com/dapr/cli#launch-dapr-and-your-app)
- [Application development with Visual Studio Code](https://github.com/dapr/docs/tree/master/howto/vscode-debugging-daprd)
- [Bindings](https://github.com/dapr/docs/tree/master/concepts/bindings)
- [Azure Service Bus Queues binding specification](https://github.com/dapr/docs/blob/master/reference/specs/bindings/servicebusqueues.md)
- [State management](https://github.com/dapr/docs/tree/master/concepts/state-management)
- [State API](https://github.com/dapr/docs/blob/master/reference/api/state_api.md)
- [Supported state stores](https://github.com/dapr/components-contrib/tree/master/state)
- [How to setup state stores](https://github.com/dapr/docs/tree/master/howto/setup-state-store)
- [Diagnose with tracing](https://github.com/dapr/docs/tree/master/howto/diagnose-with-tracing)

## Microservices port configuration for debugging

| services  | http | http-dapr | grpc-dapr | metrics-dapr | daprd command |
|---|---|---|---|---|---|
| RetroPOS.ExposedService.Api | 5000 | 5100 | 5200 | 5300 | daprd -app-id exposed-api -components-path source\workflow-1\RetroPOS.Dapr.Components -app-port 5000 -dapr-http-port 5100 -dapr-grpc-port 5200 -metrics-port 5300 -log-level debug -config source\workflow-1\RetroPOS.Dapr.Components\dapr.tracing.yml |
| RetroPOS.Consumer.Api | 6000 | 6100 | 6200 | 6300 | daprd -app-id consumer-api -components-path source\workflow-1\RetroPOS.Dapr.Components -app-port 6000 -dapr-http-port 6100 -dapr-grpc-port 6200 -metrics-port 6300 -log-level debug -config source\workflow-1\RetroPOS.Dapr.Components\dapr.tracing.yml |

## Applications to Container Registry

1. Connect to Azure Container Registry via Azure CLI.

    ```bash
    az login
    ```

2. Build, tag and push image to Azure Container Registry.

    Exposed Service API:

    ```bash
    az acr build -f source/workflow-1/RetroPOS.ExposedService.Api/Dockerfile -t [name of registry].[registry host]/exposed-api:1.0.0 -r [name of registry] source/workflow-1/RetroPOS.ExposedService.Api/
    ```

    Consumer API:

    ```bash
    az acr build -f source/workflow-1/RetroPOS.Consumer.Api/Dockerfile -t [name of registry].[registry host]/consumer-api:1.0.0 -r [name of registry] source/workflow-1/RetroPOS.Consumer.Api/
    ```

## Kubernetes resources

1. Create the retropos-workflow-1 namespace.

    ```bash
    kubectl create ns retropos-workflow-1
    ```

2. HELM Charts.

    <b>Important:</b> <u>The HELM chart deployment is ready to work with the resources deployed by using the Azure powershell script, in case you want to use other Azure resource or Dapr component you will need to modify the HELM chart.</u>

    <b>Chart:</b> retropos-system-workflow-1-dapr
    
    <b>Description:</b> HELM chart to deploy all the Dapr components used for Workflow 1

    | Required Parameters | Description |
    |-|-|
    | dapr.secretStore.vaultName | name of the key vault resource |
    | dapr.secretStore.clientId | managed identity client id |
    | dapr.bindingInternalQueue.connectionString | service bus connection string secret on key vault |
    | dapr.stateConsumerState.url | cosmosdb url |
    | dapr.stateConsumerState.masterKey | cosmosdb primary key secret on key vault |
    | dapr.stateConsumerState.database | cosmosdb database name |

    <b>Example of helm chart installation:</b>

    ```bash
    helm upgrade --install retropos-system-workflow-1-dapr kubernetes\retropos-system-workflow-1-dapr 
                 --namespace retropos-workflow-1 
                 --set dapr.secretStore.vaultName=retroposkv 
                 --set dapr.secretStore.clientId=00000000-0000-0000-0000-000000000000 
                 --set dapr.bindingInternalQueue.connectionString=servicebus-connectionstring-retropossbns 
                 --set dapr.stateConsumerState.url=https://retroposcos.documents.azure.com:443/ 
                 --set dapr.stateConsumerState.masterKey=cosmosdb-primarykey-retroposcos 
                 --set dapr.stateConsumerState.database=retroposdb
    ```

    <b>Chart:</b> retropos-system-workflow-1
    
    <b>Description:</b> HELM chart to deploy all the solution components used for Workflow 1

    | Required Parameters | Description |
    |-|-|
    | secretProviderClass.keyVaultName | name of the key vault resource |
    | secretProviderClass.resourceGroup | name of the resource group |
    | secretProviderClass.subscriptionId | subscription identifier |
    | secretProviderClass.tenantId | tenant identifier |
    | secretProviderClass.databaseConnectionString | cosmosdb connection string secret on key vault |
    | secretProviderClass.databasePrimaryKey | cosmosdb primary key secret on key vault |
    | secretProviderClass.storageConnectionString | storage connection string secret on key vault |
    | secretProviderClass.serviceBusConnectionString | service bus connection string secret on key vault |
    | exposedAPI.deployment.image.repository | repository, image and tag from the exposed service api |
    | consumerAPI.deployment.image.repository | repository, image and tag from the consumer api |
    | consumerAPI.deployment.env.azureWebJobsStorage | storage connection string secret on key vault |
    | consumerAPI.deployment.env.serviceBusConnectionString | service bus connection string secret on key vault |

    <b>Example of helm chart installation:</b>

    ```bash
    helm upgrade --install retropos-system-workflow-1 kubernetes\retropos-system-workflow-1 
                 --namespace retropos-workflow-1 
                 --set secretProviderClass.keyVaultName=retroposkv 
                 --set secretProviderClass.resourceGroup=retropos-group 
                 --set secretProviderClass.subscriptionId=00000000-0000-0000-0000-000000000000 
                 --set secretProviderClass.tenantId=00000000-0000-0000-0000-000000000000 
                 --set secretProviderClass.databaseConnectionString=cosmosdb-connectionstring-retroposcos 
                 --set secretProviderClass.databasePrimaryKey=cosmosdb-primarykey-retroposcos 
                 --set secretProviderClass.serviceBusConnectionString=servicebus-connectionstring-retropossbns 
                 --set secretProviderClass.storageConnectionString=storage-connectionstring-retroposstg 
                 --set exposedAPI.deployment.image.repository=retroposcr.azurecr.io/exposed-api:1.0.0 
                 --set consumerAPI.deployment.image.repository=retroposcr.azurecr.io/consumer-api:1.0.0 
                 --set consumerAPI.deployment.env.azureWebJobsStorage=storage-connectionstring-retroposstg 
                 --set consumerAPI.deployment.env.serviceBusConnectionString=servicebus-connectionstring-retropossbns
    ```