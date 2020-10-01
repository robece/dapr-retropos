# Kubernetes setup

## Summary

1. CSI Secrets Store Provider and AAD Pod Identity configuration
2. DAPR configuration
3. KEDA configuration
4. Zipkin configuration

## CSI Secrets Store Provider and AAD Pod Identity configuration

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

## DAPR configuration

Before continue I strongly recommend this lecture about [Dapr environment setup](https://github.com/dapr/docs/blob/master/getting-started/environment-setup.md).

Once you have properly installed Dapr in your development environment, deploy Dapr in your cluster.

```bash
dapr init --kubernetes
```

## KEDA configuration

Before continue I strongly recommend this lecture about [KEDA concepts](https://keda.sh/docs/2.0/concepts/) and [deploying KEDA](https://keda.sh/docs/2.0/deploy/).

Deploy KEDA on the cluster.

```bash
helm repo add kedacore https://kedacore.github.io/charts
helm repo update
kubectl create namespace keda
helm install keda kedacore/keda --namespace keda
```

## Zipkin configuration

Before continue I strongly recommend this lecture about [Zipkin concepts](https://zipkin.io/) and [Dapr diagnose with tracing](https://github.com/dapr/docs/tree/master/howto/diagnose-with-tracing).

Deploy Zipkin on the cluster.

```bash
kubectl create ns exporters
kubectl create deployment zipkin --image openzipkin/zipkin -n exporters
kubectl expose deployment zipkin --type LoadBalancer --port 9411 -n exporters
```