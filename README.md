# Dapr Retro Point of Sales (Backend Workflows)

## Summary

1. Workflow 1
    - Requirements
    - Code projects
    - Components required
    - Introductory documentation
    - Microservices port configuration for debugging
    - Applications to Container Registry
    - Kubernetes resources
2. Azure deployment
    - Azure resources deployment script
    - Azure resources removal script
3. Finishing Kubernetes setup
    - CSI Secrets Store Provider and AAD Pod Identity configuration
    - DAPR configuration
    - KEDA configuration
    - Zipkin configuration

## Workflow 1

The purpose of this workflow it's to represent an exposed API that will be receiving messages from an external worker service, these messages will be stored in the Azure Service Bus Queue and eventually they will be consumed and processed by a ServiceBusTrigger Function to finally send the results to an storage in the Dapr sidecar component to be successfully saved.

<div>
    <img src="resources/images/architecture-workflow-1.png" width="800" />
</div>

In this scenario, some KEDA components were added, to horizontal autoscale the pods based on the amount of events received in the queue, this behavior will be only available in Kubernetes.

Note: During the development of the project, I found some early adoption considerations, the first one, it's the lack of the support of Dapr queue trigger for Service Bus Queues, this forced to me to use the ServiceBusTrigger Function instead of a possible DaprQueueTrigger, this creates a direct dependency to the Service Bus resource, but once we have the Dapr queue trigger available for Functions, the resource will be fully adaptive to other non-Azure components, for further details about the Dapr Functions Extension project: https://github.com/dapr/azure-functions-extension.

### Requirements

- Visual Studio 2019 or Visual Studio Code
- .NET Core 3.1
- Azure Functions Extensions
- Docker Desktop
- Dapr
- HELM

### Code projects

- source/workflow-1/RetroPOS.ExposedService.Api
- source/workflow-1/RetroPOS.ExposedService.WorkerService
- source/workflow-1/RetroPOS.Consumer.Api

### Components required

- Output Binding (Dapr) - Azure Service Bus
- State (Dapr) - Redis, CosmosDB or any other supported component by Dapr
- Distributed Tracing (Dapr) - Zipkin, App Insights or any other supported component by Dapr

### Introductory documentation

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

### Microservices port configuration for debugging

| services  | http | http-dapr | grpc-dapr | metrics-dapr | daprd command |
|---|---|---|---|---|---|
| RetroPOS.ExposedService.Api | 5000 | 5100 | 5200 | 5300 | daprd -app-id exposed-api -components-path source\workflow-1\RetroPOS.Dapr.Components -app-port 5000 -dapr-http-port 5100 -dapr-grpc-port 5200 -metrics-port 5300 -log-level debug -config source\workflow-1\RetroPOS.Dapr.Components\dapr.tracing.yml |
| RetroPOS.Consumer.Api | 6000 | 6100 | 6200 | 6300 | daprd -app-id consumer-api -components-path source\workflow-1\RetroPOS.Dapr.Components -app-port 6000 -dapr-http-port 6100 -dapr-grpc-port 6200 -metrics-port 6300 -log-level debug -config source\workflow-1\RetroPOS.Dapr.Components\dapr.tracing.yml |

### Applications to Container Registry

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

### Kubernetes resources

1. Create the retropos-workflow-1 namespace.

    ```bash
    kubectl create ns retropos-workflow-1
    ```

2. HELM Charts.

    The HELM chart deployment are based by the Azure deployment powershell scripts, in case you want to use other Dapr component you will need to modify the HELM chart. 

    Chart: retropos-system-workflow-1-dapr
    
    Description: HELM chart to deploy all the Dapr components used for Workflow 1

    Parameters:

    - dapr.secretStore.vaultName - name of the keyvault resource
    - dapr.secretStore.clientId - managed identity client id
    - dapr.bindingInternalQueue.connectionString - service bus connection string secret on keyvault
    - dapr.stateConsumerState.url - cosmosdb url
    - dapr.stateConsumerState.masterKey - cosmosdb primary key secret on keyvault
    - dapr.stateConsumerState.database - cosmosdb databasename

    Example:

    ```bash
    helm upgrade --install retropos-system-workflow-1-dapr kubernetes\retropos-system-workflow-1-dapr --namespace retropos-workflow-1 --set dapr.secretStore.vaultName=retroposkv --set dapr.secretStore.clientId=00000000-0000-0000-0000-000000000000 --set dapr.bindingInternalQueue.connectionString=servicebus-connectionstring-retropossbns --set dapr.stateConsumerState.url=https://retroposcos.documents.azure.com:443/ --set dapr.stateConsumerState.masterKey=cosmosdb-primarykey-retroposcos --set dapr.stateConsumerState.database=retroposdb
    ```

    Chart: retropos-system-workflow-1
    
    Description: HELM chart to deploy all the solution components used for Workflow 1

    Parameters:

    - secretProviderClass.keyvaultName - name of the keyvault resource
    - secretProviderClass.resourceGroup - name of the resource group
    - secretProviderClass.subscriptionId - subscription identifier
    - secretProviderClass.tenantId - tenant identifier
    - secretProviderClass.databaseConnectionString - cosmosdb connection string secret on keyvault
    - secretProviderClass.databasePrimaryKey - cosmosdb primary key secret on keyvault
    - secretProviderClass.storageConnectionString - storage connection string secret on keyvault
    - secretProviderClass.serviceBusConnectionString - service bus connection string secret on keyvault
    - exposedAPI.deployment.image.repository - repository, image and tag from the exposed service api
    - consumerAPI.deployment.image.repository - repository, image and tag from the consumer api
    - consumerAPI.deployment.env.azureWebJobsStorage - storage connection string secret on keyvault
    - consumerAPI.deployment.env.serviceBusConnectionString - service bus connection string secret on keyvault

    Example:

    ```bash
    helm upgrade --install retropos-system-workflow-1 kubernetes\retropos-system-workflow-1 --namespace retropos-workflow-1 --set secretProviderClass.keyvaultName=retroposkv --set secretProviderClass.resourceGroup=retropos-group --set secretProviderClass.subscriptionId=00000000-0000-0000-0000-000000000000 --set secretProviderClass.tenantId=00000000-0000-0000-0000-000000000000 --set secretProviderClass.databaseConnectionString=cosmosdb-connectionstring-retroposcos --set secretProviderClass.databasePrimaryKey=cosmosdb-primarykey-retroposcos --set secretProviderClass.serviceBusConnectionString=servicebus-connectionstring-retropossbns --set secretProviderClass.storageConnectionString=storage-connectionstring-retroposstg --set exposedAPI.deployment.image.repository=retroposcr.azurecr.io/exposed-api:1.0.0 --set consumerAPI.deployment.image.repository=retroposcr.azurecr.io/consumer-api:1.0.0 --set consumerAPI.deployment.env.azureWebJobsStorage=storage-connectionstring-retroposstg --set consumerAPI.deployment.env.serviceBusConnectionString=servicebus-connectionstring-retropossbns
    ```

## Azure Deployment

### Script for Azure resources deployment

Run the script: powershell/azure-deployment.ps1 to deploy the Azure resources.

```bash
.\powershell\azure-deployment.ps1
```

Note: At the end of the script execution there is a call to action to save the information that will be used in the future, be sure keep safe that information.

### Script for Azure resources removal

Run the script: powershell/azure-removal.ps1 to remove the Azure resources.
    
```bash
.\powershell\azure-removal.ps1
```

## Finishing Kubernetes setup

### CSI Secrets Store Provider and AAD Pod Identity configuration

Once the Azure services has been deployed, Azure Kubernetes Service is now configured with Managed Identity service with the grants to access the Container Registry and Azure Key Vault. 

Now it's time to configure the rest of the AAD Pod Identity and CSI Secrets Store Provider to connect Kubernetes in a secure way to Key Vault and use the managed secrets.

Steps:

1. Using the Azure CLI download the cluster credentials in the local environment or if you prefer use the Azure Shell.

    ```bash
    az aks get-credentials -n [Kubernetes Service name] -g [Kubernetes Service resource group]
    ```

2. Using the Azure CLI install the Kubectl command line interface.

    ```bash
    az aks install-cli
    ```

3. Install Secrets Store CSI driver and Key Vault Provider.
    
    ```bash
    helm repo add csi-secrets-store-provider-azure https://raw.githubusercontent.com/Azure/secrets-store-csi-driver-provider-azure/master/charts
    kubectl create ns csi-driver
    helm install csi-azure csi-secrets-store-provider-azure/csi-secrets-store-provider-azure --namespace csi-driver
    ```

4. Install Aad-Pod-Identity on Kubernetes.

    ```bash
    kubectl apply -f https://raw.githubusercontent.com/Azure/aad-pod-identity/master/deploy/infra/deployment-rbac.yaml
    ```

    Note: For more information: https://github.com/Azure/aad-pod-identity.

5. Run the script: powershell/aad-permissions.ps1 to assign permissions.

    To run the script you need to modified the following values with the right ones before execute the script.

    ```bash
    - SubscriptionId="[REPLACE]"
    - KubernetesName="[REPLACE]"
    - KeyVaultName="[REPLACE]"
    - ResourceGroupName="[REPLACE]"
    ```

    ```bash
    .\powershell\aad-permissions.ps1
    ```

6. Run the script: kubernetes/aad-pod-identity/kubernetes.azureidentity.yml.

    To install the script you need to configure the identity resourceID and clientID before execute the script.

    ```bash
    kubectl apply -f kubernetes/aad-pod-identity/kubernetes.azureidentity.yml
    ```

7. Run the script: kubernetes/aad-pod-identity/kubernetes.azureidentitybinding.yml.

    ```bash
    kubectl apply -f kubernetes/aad-pod-identity/kubernetes.azureidentitybinding.yml
    ```

### DAPR configuration

Before continue I strongly recommend this lecture about [Dapr environment setup](https://github.com/dapr/docs/blob/master/getting-started/environment-setup.md).

Once you have properly installed Dapr in your development environment, deploy Dapr in your cluster.

```bash
dapr init --kubernetes
```

### KEDA configuration

Before continue I strongly recommend this lecture about [KEDA concepts](https://keda.sh/docs/2.0/concepts/) and [deploying KEDA](https://keda.sh/docs/2.0/deploy/).

Deploy KEDA on the cluster.

```bash
helm repo add kedacore https://kedacore.github.io/charts
helm repo update
kubectl create namespace keda
helm install keda kedacore/keda --namespace keda
```

### Zipkin configuration

Before continue I strongly recommend this lecture about [Zipkin concepts](https://zipkin.io/) and [Dapr diagnose with tracing](https://github.com/dapr/docs/tree/master/howto/diagnose-with-tracing).

Deploy Zipkin on the cluster.

```bash
kubectl create ns exporters
kubectl create deployment zipkin --image openzipkin/zipkin -n exporters
kubectl expose deployment zipkin --type LoadBalancer --port 9411 -n exporters
```

## Credits

I want to thank to my Cloud Native Global Black Belt team (aka. Dapr Vigilantes) for this great contribution: https://github.com/azure/dapr-gbb-workshop that I used to accelerate my learning and adoption of Dapr.

I want to thank Houssem Dellai (@houssemdellai) for this great contribution: https://github.com/houssemdellai/aks-keyvault that I used to create part of the AAD Pod Identity documentation.