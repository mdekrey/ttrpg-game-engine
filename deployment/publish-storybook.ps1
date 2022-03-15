
Push-Location "$PSScriptRoot/.."

$tag = (Get-Date).ToString('yyyy-MM-ddTHH_mm_ss')

docker build . -f .\deployment\Dockerfile.storybook -t dekreydotnet.azurecr.io/game-4e-storybook:$tag

az acr login --name dekreydotnet
docker push dekreydotnet.azurecr.io/game-4e-storybook:$tag

Pop-Location


$ns = 'game-engine'
$name = 'storybook'
$fullImageName = 'dekreydotnet.azurecr.io/game-4e-storybook'
$domain = 'storybook.4e.dekrey.net'

helm upgrade --install -n $ns $name --create-namespace mdekrey/single-container `
     --set-string "image.repository=$($fullImageName)" `
     --set-string "image.tag=$tag" `
     --set-string "ingress.annotations.cert-manager\.io/cluster-issuer=letsencrypt" `
     --set-string "ingress.hosts[0].host=$domain"