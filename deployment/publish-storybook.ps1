
Push-Location "$PSScriptRoot/.."

$tag = (Get-Date).ToString('yyyy-MM-ddTHH_mm_ss')

docker build . -f .\deployment\Dockerfile.storybook -t dekreydotnet.azurecr.io/game-4e-storybook:$tag

az acr login -name dekreydotnet
docker push dekreydotnet.azurecr.io/game-4e-storybook:$tag

Pop-Location
