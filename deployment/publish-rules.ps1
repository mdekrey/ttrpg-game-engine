
Push-Location "$PSScriptRoot/.."

$tag = (Get-Date).ToString('yyyy-MM-ddTHH_mm_ss')

docker build . -f .\deployment\Dockerfile -t dekreydotnet.azurecr.io/game-4e:$tag

az acr login --name dekreydotnet
docker push dekreydotnet.azurecr.io/game-4e:$tag

Pop-Location

kubectl -n game-engine set image deployment game-deployment web=dekreydotnet.azurecr.io/game-4e:$tag