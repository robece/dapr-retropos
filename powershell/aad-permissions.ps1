$SubscriptionId="[REPLACE]"
$KubernetesName="[REPLACE]"
$KeyVaultName="[REPLACE]"
$ResourceGroupName="[REPLACE]"

$aks=$(az aks show -n $KubernetesName -g $ResourceGroupName | ConvertFrom-Json)

# assing permission to Managed Identity Operator
az role assignment create --role "Managed Identity Operator" --assignee $aks.identityProfile.kubeletidentity.clientId --scope /subscriptions/$SubscriptionId/resourcegroups/$($aks.nodeResourceGroup)

# assing permission to Virtual Machine Contributor
az role assignment create --role "Virtual Machine Contributor" --assignee $aks.identityProfile.kubeletidentity.clientId --scope /subscriptions/$SubscriptionId/resourcegroups/$($aks.nodeResourceGroup)

$identity=$(az identity show -n $KubernetesName"-agentpool" -g $aks.nodeResourceGroup | ConvertFrom-Json)

# assign policy to access secrets in Key Vault
az keyvault set-policy -n $KeyVaultName -g $ResourceGroupName --secret-permissions get list --spn $identity.clientId