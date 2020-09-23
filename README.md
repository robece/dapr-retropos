# Dapr Retro Point of Sales

<div style="text-align:center">
    <img src="/resources/images/architecture.png" />
</div>

## Microservices port configuration for debugging

| services  | http | http-dapr | grpc-dapr | metrics-dapr | daprd command |
|---|---|---|---|---|---|
| RetroPOS.Warehouse.Api | 5000 | 5100 | 5200 | 5300 | daprd -app-id warehouse-service -components-path source\RetroPOS.Dapr.Components -app-port 5000 -dapr-grpc-port 5200 -dapr-http-port 5100 -metrics-port 5300 -log-level debug -config source\RetroPOS.Dapr.Components\retropos.observability.tracing.yml |
| RetroPOS.Audit.Api | 6000 | 6100 | 6200 | 6300 | daprd -app-id audit-service -components-path source\RetroPOS.Dapr.Components -app-port 6000 -dapr-grpc-port 6200 -dapr-http-port 6100 -metrics-port 6300 -log-level debug -config source\RetroPOS.Dapr.Components\retropos.observability.tracing.yml |
| RetroPOS.DurableOrchestration.Api | 7000 | 7100 | 7200 | 7300 | daprd -app-id durable-orchestration-api -components-path kubernetes -app-port 7000 -dapr-grpc-port 7200 -dapr-http-port 7100 -metrics-port 7300 -log-level debug |
| RetroPOS.Consumer.Api | 8000 | 8100 | 8200 | 8300 | daprd -app-id consumer-api -components-path kubernetes -app-port 8000 -dapr-grpc-port 8200 -dapr-http-port 8100 -metrics-port 8300 -log-level debug |

## Script for Azure resources deployment

Run the script: powershell/azure-deployment.ps1 to deploy the Azure resources.

```bash
.\powershell\azure-deployment.ps1
```

Note: At the end of the script execution there is a call to action to save the information that will be used in the future, be sure keep safe that information.

## Script for Azure resources removal

Run the script: powershell/azure-removal.ps1 to remove the Azure resources.
    
```bash
.\powershell\azure-removal.ps1
```

## Finishing the AAD Pod Identity configuration on Kubernetes

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

## Deploying the applications

1. Create the retropos-system namespace.

    ```bash
    kubectl create ns retropos-system
    ```

## Credits

I want to thank to my Cloud Native Global Black Belt team (aka. Dapr Vigilantes) for this great contribution: https://github.com/azure/dapr-gbb-workshop that I used to accelerate my learning and adoption of Dapr.

I want to thank Houssem Dellai (@houssemdellai) for this great contribution: https://github.com/houssemdellai/aks-keyvault that I used to create part of the AAD Pod Identity documentation.