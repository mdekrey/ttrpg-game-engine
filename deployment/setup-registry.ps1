
Param(
    [Parameter(Mandatory=$true)]
    [String]
    $subscriptionId,
    [Parameter(Mandatory=$true)]
    [String]
    $resourceGroup,
    [Parameter(Mandatory=$true)]
    [String]
    $registryName
)

$role = az ad sp create-for-rbac --scopes /subscriptions/$($subscriptionId)/resourcegroups/$($resourceGroup)/providers/Microsoft.ContainerRegistry/registries/$($registryName) --role Reader --name gameengine-image-reader `
    | ConvertFrom-Json

kubectl -n game-engine delete secret game-engine-registry --ignore-not-found=true
kubectl -n game-engine create secret docker-registry game-engine-registry --docker-server "$($registryName).azurecr.io" --docker-username=$($role.appId) --docker-password $($role.password)
