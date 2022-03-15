
Push-Location "$PSScriptRoot/.."

$tag = (Get-Date).ToString('yyyy-MM-ddTHH_mm_ss')

docker build . -f .\deployment\Dockerfile -t dekreydotnet.azurecr.io/game-4e:$tag

az account set --subscription 2351fc7a-207c-4a7d-8104-d5fe21d7907f

az acr login --name dekreydotnet
docker push dekreydotnet.azurecr.io/game-4e:$tag

$ns = 'game-engine'
$name = 'main'
$fullImageName = 'dekreydotnet.azurecr.io/game-4e'
$sslClusterIssuer = 'letsencrypt'
$domain = '4e.dekrey.net'

helm upgrade --install -n $ns $name --create-namespace mdekrey/single-container `
    --set-string "image.repository=$($fullImageName)" `
    --set-string "image.tag=$tag" `
    --set-string "ingress.annotations.cert-manager\.io/cluster-issuer=$sslClusterIssuer" `
    --set-string "ingress.hosts[0].host=$domain" `
    --set-string "podspec.env[0].name=BlobStorage,podspec.env[0].valueFrom.secretKeyRef.key=BlobStorage,podspec.env[0].valueFrom.secretKeyRef.name=environment-settings"

Pop-Location
