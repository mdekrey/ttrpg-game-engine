Push-Location "$PSScriptRoot/.."

docker run --rm -ti -p 10000:10000 -p 10001:10001 -p 10002:10002 `
    -v "$($(pwd).ToString().Replace('\', '/'))/ecf:/data" `
    mcr.microsoft.com/azure-storage/azurite


Pop-Location
