param (
    [String]
    $subscription = '2351fc7a-207c-4a7d-8104-d5fe21d7907f',

    [String]
    $repository = 'dekreydotnet',
    [String]
    $imageName = 'game-4e',

    [String]
    $azureResourceGroup = 'DeKreyDotNet',
    [String]
    $azureAksCluster = 'TinyKubed',

    [String]
    $k8sNamespace = 'game-engine',
    [String]
    $chartName = 'main'
)

Push-Location "$PSScriptRoot/.."

$tag = (Get-Date).ToString('yyyy-MM-ddTHH_mm_ss')
$fullImageName = "$($repository).azurecr.io/$($imageName)"

docker build . -f .\deployment\Dockerfile -t dekreydotnet.azurecr.io/game-4e:$tag

az account set --subscription $($subscription)

az acr login --name dekreydotnet
docker push dekreydotnet.azurecr.io/game-4e:$tag

$sslClusterIssuer = 'letsencrypt'
$domain = '4e.dekrey.net'

az aks get-credentials --resource-group $azureResourceGroup -n $azureAksCluster
helm upgrade --install -n $k8sNamespace $chartName --create-namespace mdekrey/single-container `
    --set-string "image.repository=$($fullImageName)" `
    --set-string "image.tag=$tag" `
    --set-string "ingress.annotations.cert-manager\.io/cluster-issuer=$sslClusterIssuer" `
    --set-string "ingress.hosts[0].host=$domain" `
    --set-string "podspec.env[0].name=BlobStorage,podspec.env[0].valueFrom.secretKeyRef.key=BlobStorage,podspec.env[0].valueFrom.secretKeyRef.name=environment-settings"

Pop-Location
