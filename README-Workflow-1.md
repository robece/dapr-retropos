# Workflow 1 - API to Queue / Queue to Function

## Summary

1. [Description](#description)
2. [Code projects](#code-projects)
3. [Components required](#components-required)
4. [Introductory documentation](#introductory-documentation)
5. [Microservices port configuration for debugging](#microservices-port-configuration-for-debugging)
6. [Applications to Container Registry](#applications-to-container-registry)
7. [Kubernetes resources](#kubernetes-resources)
8. [Load test](#load-test)

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
- source/workflow-1/RetroPOS.Consumer.Function

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

## Microservices port configuration for local development environment 

| services  | http | http-dapr | grpc-dapr | dapr command |
|---|---|---|---|---|
| RetroPOS.ExposedService.Api | 5000 | 5100 | 5200 | dapr run --app-id exposed-api --components-path source\workflow-1\RetroPOS.Dapr.Components --app-port 5000 --dapr-http-port 5100 --dapr-grpc-port 5200 --config source\workflow-1\RetroPOS.Dapr.Components\dapr.tracing.yml |
| RetroPOS.Consumer.Function | 6000 | 6100 | 6200 | dapr run --app-id consumer-function --components-path source\workflow-1\RetroPOS.Dapr.Components --app-port 6000 --dapr-http-port 6100 --dapr-grpc-port 6200 --config source\workflow-1\RetroPOS.Dapr.Components\dapr.tracing.yml |

## Applications to Container Registry

1. Connect to Azure Container Registry via Azure CLI.

    ```
    az login
    ```

2. Build, tag and push image to Azure Container Registry.

    Exposed Service API:

    ```
    az acr build -f source/workflow-1/RetroPOS.ExposedService.Api/Dockerfile -t [name of registry].[registry host]/exposed-api:1.0.0 -r [name of registry] source/workflow-1/RetroPOS.ExposedService.Api/
    ```

    Consumer Function:

    ```
    az acr build -f source/workflow-1/RetroPOS.Consumer.Function/Dockerfile -t [name of registry].[registry host]/consumer-function:1.0.0 -r [name of registry] source/workflow-1/RetroPOS.Consumer.Function/
    ```

## Kubernetes resources

1. Create the retropos-workflow-1 namespace.

    ```
    kubectl create ns retropos-workflow-1
    ```

2. HELM Charts.

    >  ### Installation order:
    > 1. security-components
    > 2. dapr-components
    > 3. exposed-api
    > 4. consumer-function
    >
    > ### Important: 
    > Packages has been prepared to work with the resources deployed with the Azure powershell script, in case you want to use another component you will need to modify the target chart.

    <b>Chart:</b> security-components
    
    <b>Description:</b> Package to deploy security components for Workflow 1

    | Required Parameters | Description |
    |-|-|
    | secretProviderClass.secretObjects.databaseConnectionString | cosmosdb connection string secret on key vault |
    | secretProviderClass.secretObjects.databasePrimaryKey | cosmosdb primary key secret on key vault |
    | secretProviderClass.secretObjects.serviceBusConnectionString | service bus connection string secret on key vault |
    | secretProviderClass.secretObjects.storageConnectionString | storage connection string secret on key vault |
    | secretProviderClass.secretObjects.storagePrimaryKey | storage primary key secret on key vault |
    | secretProviderClass.parameters.keyVaultName | name of the key vault resource |
    | secretProviderClass.parameters.resourceGroup | name of the resource group |
    | secretProviderClass.parameters.subscriptionId | subscription identifier |
    | secretProviderClass.parameters.tenantId | tenant identifier |

    <b>Chart installation sample:</b>

    ```
    helm upgrade --install security-components kubernetes\helm\workflow-1\security-components 
                 --namespace retropos-workflow-1 
                 --set secretProviderClass.secretObjects.databaseConnectionString=cosmosdb-connectionstring-retroposcos 
                 --set secretProviderClass.secretObjects.databasePrimaryKey=cosmosdb-primarykey-retroposcos 
                 --set secretProviderClass.secretObjects.serviceBusConnectionString=servicebus-connectionstring-retropossbns 
                 --set secretProviderClass.secretObjects.storageConnectionString=storage-connectionstring-retroposstg 
                 --set secretProviderClass.secretObjects.storagePrimaryKey=storage-primarykey-retroposstg 
                 --set secretProviderClass.parameters.keyVaultName=retroposkv 
                 --set secretProviderClass.parameters.resourceGroup=retropos-group 
                 --set secretProviderClass.parameters.subscriptionId=00000000-0000-0000-0000-000000000000 
                 --set secretProviderClass.parameters.tenantId=00000000-0000-0000-0000-000000000000
    ```

    <b>Chart:</b> dapr-components
    
    <b>Description:</b> Package to deploy dapr components for Workflow 1

    | Required Parameters | Description |
    |-|-|
    | secretStore.vaultName | name of the key vault resource |
    | secretStore.clientId | managed identity client id |
    | bindingInternalQueue.connectionString | service bus connection string secret on key vault |
    | stateConsumerState.url | cosmosdb url |
    | stateConsumerState.masterKey | cosmosdb primary key secret on key vault |
    | stateConsumerState.database | cosmosdb database name |

    <b>Chart installation sample:</b>

    ```
    helm upgrade --install dapr-components kubernetes\helm\workflow-1\dapr-components 
                 --namespace retropos-workflow-1 
                 --set secretStore.vaultName=retroposkv 
                 --set secretStore.clientId=00000000-0000-0000-0000-000000000000 
                 --set bindingInternalQueue.connectionString=servicebus-connectionstring-retropossbns 
                 --set stateConsumerState.url=https://retroposcos.documents.azure.com:443/ 
                 --set stateConsumerState.masterKey=cosmosdb-primarykey-retroposcos 
                 --set stateConsumerState.database=retroposdb
    ```

    <b>Chart:</b> exposed-api
    
    <b>Description:</b> Package to deploy exposed api components for Workflow 1

    | Required Parameters | Description |
    |-|-|
    | deployment.replicas | exposed service api pod replicas |
    | deployment.image.repository | exposed service api repository, image and tag |
    | autoscaling.minReplicas | exposed service api autoscaler pod min replicas |
    | autoscaling.maxReplicas | exposed service api autoscaler pod max replicas |

    <b>Chart installation sample:</b>

    ```
    helm upgrade --install exposed-api kubernetes\helm\workflow-1\exposed-api 
                 --namespace retropos-workflow-1 
                 --set deployment.replicas=2 
                 --set deployment.image.repository=retroposcr.azurecr.io/exposed-api:1.0.0 
                 --set autoscaling.minReplicas=2 
                 --set autoscaling.maxReplicas=20
    ```

    <b>Chart:</b> consumer-function
    
    <b>Description:</b> Package to deploy consumer-function components for Workflow 1

    | Required Parameters | Description |
    |-|-|
    | deployment.replicas | consumer function pod replicas |
    | deployment.image.repository | consumer function repository, image and tag |
    | deployment.env.azureWebJobsStorage | consumer function storage connection string secret on key vault |
    | deployment.env.serviceBusConnectionString | consumer function service bus connection string secret on key vault |
    | keda.scaledObject.minReplicaCount | consumer function keda min pod replicas |
    | keda.scaledObject.maxReplicaCount | consumer function keda max pod replicas |
    | keda.scaledObject.triggers.messageCount | consumer function keda message count |

    <b>Chart installation sample:</b>

    ```
    helm upgrade --install consumer-function kubernetes\helm\workflow-1\consumer-function 
                 --namespace retropos-workflow-1 
                 --set deployment.replicas=5 
                 --set deployment.image.repository=retroposcr.azurecr.io/consumer-function:1.0.0 
                 --set deployment.env.azureWebJobsStorage=storage-connectionstring-retroposstg 
                 --set deployment.env.serviceBusConnectionString=servicebus-connectionstring-retropossbns 
                 --set keda.scaledObject.minReplicaCount=5 
                 --set keda.scaledObject.maxReplicaCount=20 
                 --set keda.scaledObject.triggers.messageCount=2
    ```

## Load test

This is an important consideration in the workflow design to validate the stability, integrity and resilence of each one of the components, because the more components we have the more points of failures we need to validate. You can perform this by using the RetroPOS.ExposedService.WorkerService project, this is console application that can run in the development environment to hit our cluster or we can use a set of virtual machines working together to amplify the volumetry of requests.