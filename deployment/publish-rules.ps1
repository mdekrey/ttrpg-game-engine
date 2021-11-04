
Push-Location "$PSScriptRoot/.."

$tag = (Get-Date).ToString('yyyy-MM-ddTHH_mm_ss')

docker build . -f .\deployment\Dockerfile -t dekreydotnet.azurecr.io/game-4e:$tag

az account set --subscription 2351fc7a-207c-4a7d-8104-d5fe21d7907f

az acr login --name dekreydotnet
docker push dekreydotnet.azurecr.io/game-4e:$tag

Pop-Location

az aks get-credentials --resource-group DeKreyDotNet -n TinyKubed
kubectl -n game-engine set image deployment game-deployment web=dekreydotnet.azurecr.io/game-4e:$tag