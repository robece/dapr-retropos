# NOTE: before run this script ensure you are logged in Azure by using "az login" command.

$DeploymentAlias = Read-Host -Prompt "Introduce a lowercase unique alias of the deployment you want to cleanup (max length suggested of 9 chars)"
$AKSClusterName = "$($DeploymentAlias)ks"
$ResourceGroupName = "$($DeploymentAlias)-group"

Write-Host "Operation: Delete resource group $($ResourceGroupName)" 
# delete delete general resource group
az group delete -n $ResourceGroupName --no-wait

# get cluster information
$aks=$(az aks show -n $AKSClusterName -g $ResourceGroupName | ConvertFrom-Json)

Write-Host "Operation: Delete resource group $($aks.nodeResourceGroup)" 
# delete delete aks node group resource group
az group delete -n $aks.nodeResourceGroup --no-wait