
First:

    kubectl apply -f ./deployment/k8s/ns.yaml

To find your subscription ID:

    az account list

Setting up the container registry:

    ./deployment/setup-registry.ps1 -subscriptionId <SubscriptionID> -resourceGroup <ResourceGroup> -registryName <ContainerRegistry>

Then, publish your initial images and create the deployments.

    docker build . -f .\deployment\Dockerfile.storybook -t dekreydotnet.azurecr.io/game-4e-storybook
    docker push dekreydotnet.azurecr.io/game-4e-storybook
    kubectl apply -f ./deployment/k8s/storybook.yaml

    docker build . -f .\deployment\Dockerfile -t dekreydotnet.azurecr.io/game-4e
    docker push dekreydotnet.azurecr.io/game-4e
    kubectl apply -f ./deployment/k8s/site.yaml

