############################################################
#          Login to azure with your user                   #
#                                                          #
############################################################

az login --use-device-code

#############################################################
##          see which avilable locations we have            #
##                                                          #
#############################################################

$locations = az account list-locations | ConvertFrom-Json
$choosenLocation =  $locations[0].name
echo $choosenLocation

$randomIdentifier= "1" #Get-Random // can add this flag to generate a unique name each deployment
$location=$choosenLocation
$resourceGroup="HomeWork2-cloudcomputing-rg-$randomIdentifier"
$tag="deploy-github"
$gitrepoQueue="https://github.com/evyatarweiss/HW2_QueueAPI" 
$appServicePlan="Cloud-ComputingHW2-$randomIdentifier"
$webappQueue="hw2CloudComputing-queue-$randomIdentifier"

#############################################################
##          Create a resource group                         #
##                                                          #
#############################################################
echo "Creating $resourceGroup in "$location"..."
$CreateResourceGroup = az group create --name $resourceGroup --location "$location" --tag $tag | ConvertFrom-Json

echo $CreateResourceGroup

#############################################################
##         Create an App Service plan in `FREE` tier.       #
##                                                          #
#############################################################
echo "Creating $appServicePlan"
$AppservicePlanSub = az appservice plan create --name $appServicePlan --resource-group $resourceGroup --sku FREE | ConvertFrom-Json

#############################################################
##         #1 Create a web app for the queue     +          #
## #2 Deploy the Queue code from a public GitHub repository #
#############################################################

#1
echo "Creating $webappQueue"
$QueueAPIwebApp = az webapp create --name $webappQueue --resource-group $resourceGroup --plan $appServicePlan | ConvertFrom-Json

#2
az webapp deployment source config --name $webappQueue --resource-group $resourceGroup `
--repo-url $gitrepoQueue --branch main --manual-integration

#############################################################
##        #1 Deploying first QueueService Api               #
##        #2 Changing the API appsettings.json              #
#############################################################

#1

$webappAPI="hw2-web-app-API-$randomIdentifier"
# Create a web app for API.

echo "Creating $webappAPI"
$QueueServiceApiNodeA = az webapp create --name $webappAPI --resource-group $resourceGroup --plan $appServicePlan | ConvertFrom-Json

$gitrepoAPI="https://github.com/evyatarweiss/HW2_QueueServiceAPI" 

#Deploy webApp API
az webapp deployment source config --name $webappAPI --resource-group $resourceGroup `
--repo-url $gitrepoAPI --branch main --manual-integration 

#2
$webAppSettings =  az webapp config appsettings list --name $webappAPI --resource-group $resourceGroup
$appsettingList=$webAppSettings.SiteConfig.AppSettings

#current appsettings
$appsettingList
$QueueHostForAPI = "https://" + $QueueAPIwebApp.defaultHostName
az webapp config appsettings set -g $resourceGroup -n $webappAPI --settings QueueHost=$QueueHostForAPI

#############################################################
##        #1 Deploying second QueueService Api              #
##        #2 Changing the API appsettings.json              #
#############################################################

#1

$webappAPI2="hw2-web-app-API2-$randomIdentifier"
# Create a web app for API.

echo "Creating $webappAPI2"
$QueueServiceApiNodeB = az webapp create --name $webappAPI2 --resource-group $resourceGroup --plan $appServicePlan | ConvertFrom-Json

$gitrepoAPI="https://github.com/evyatarweiss/HW2_QueueServiceAPI" 

#Deploy webApp API
az webapp deployment source config --name $webappAPI2 --resource-group $resourceGroup `
--repo-url $gitrepoAPI --branch main --manual-integration 

$webAppSettings =  az webapp config appsettings list --name $webappAPI2 --resource-group $resourceGroup
$appsettingList=$webAppSettings.SiteConfig.AppSettings

#current appsettings
$appsettingList
$QueueHostForAPI2 = "https://" + $QueueAPIwebApp.defaultHostName
az webapp config appsettings set -g $resourceGroup -n $webappAPI2 --settings QueueHost=$QueueHostForAPI2

#############################################################
##         Create an WebApp for the worker node             #
##                                                          #
#############################################################

$webappWN="hw2-web-app-WorkerNode-$randomIdentifier"
# Create a web app for API.

echo "Creating $webappWN"
$WorkerNodeWebApp = az webapp create --name $webappWN --resource-group $resourceGroup --plan $appServicePlan | ConvertFrom-Json 

$gitrepoWN="https://github.com/evyatarweiss/HW2_WorkerNode" 

#Deploy webApp API
az webapp deployment source config --name $webappWN --resource-group $resourceGroup `
--repo-url $gitrepoWN --branch main --manual-integration 

#2
$webAppSettings =  az webapp config appsettings list --name $webappWN --resource-group $resourceGroup
$appsettingList=$webAppSettings.SiteConfig.AppSettings

#current appsettings
$appsettingList
$QueueHostForWN = "https://" + $QueueAPIwebApp.defaultHostName
az webapp config appsettings set -g $resourceGroup -n $webappWN --settings QueueHost=$QueueHostForWN

$startWorkingUrl = "https://"+$WorkerNodeWebApp.defaultHostName+"/startWorking"
Invoke-WebRequest $startWorkingUrl -Method 'GET'

#############################################################
##         Create an WebApp for the Traffic Manager         #
##                                                          #
#############################################################

$webappTM="hw2-web-app-Traffic-Manager-$randomIdentifier"
# Create a web app for API.

echo "Creating $webappTM"
az webapp create --name $webappTM --resource-group $resourceGroup --plan $appServicePlan 

$gitrepoTM="https://github.com/evyatarweiss/HW2_TrafficManager" 

#Deploy webApp API
az webapp deployment source config --name $webappTM --resource-group $resourceGroup `
--repo-url $gitrepoTM --branch main --manual-integration 

####Please note that there is a pop-up window here####

$webAppSettings =  az webapp config appsettings list --name $webappTM --resource-group $resourceGroup
$appsettingList=$webAppSettings.SiteConfig.AppSettings

#current appsettings
$appsettingList
$ParameterQueueServiceApiNodeA = "https://" + $QueueServiceApiNodeA.defaultHostName
$ParameterQueueServiceApiNodeB = "https://" + $QueueServiceApiNodeB.defaultHostName

az webapp config appsettings set -g $resourceGroup -n $webappTM --settings QueueServiceApiNodeA=$ParameterQueueServiceApiNodeA QueueServiceApiNodeB=$ParameterQueueServiceApiNodeB



#############################################################
##         Create an WebApp for the Scale -Manager          #
##                                                          #
#############################################################

$webappSM="hw2-web-app-Scale-Manager-$randomIdentifier"
# Create a web app for API.

echo "Creating $webappSM"
az webapp create --name $webappSM --resource-group $resourceGroup --plan $appServicePlan 

$gitrepoSM="https://github.com/evyatarweiss/HW2_ScaleManager" 

#Deploy webApp API
az webapp deployment source config --name $webappSM --resource-group $resourceGroup `
--repo-url $gitrepoSM --branch main --manual-integration 



$ServicePrincipalScopes = "/subscriptions/" + $AppservicePlanSub.subscription

$webAppSettings =  az webapp config appsettings list --name $webappSM --resource-group $resourceGroup
$appsettingList=$webAppSettings.SiteConfig.AppSettings

#current appsettings
$appsettingList

Creating a service principal and grant permissions to the subscription
$ServicePrincipal = az ad sp create-for-rbac --role "Owner" --scopes $ServicePrincipalScopes | ConvertFrom-Json

modify the settings
$tenantId = $ServicePrincipal.tenant
$clientId = $ServicePrincipal.appId
$clientSecret = $ServicePrincipal.password
$queueHost = "https://"+$QueueAPIwebApp.defaultHostName
$subscriptionId = $AppservicePlanSub.subscription

az webapp config appsettings set -g $resourceGroup -n $webappSM --settings tenantId=$tenantId clientId=$clientId clientSecret=$clientSecret subscriptionId=$subscriptionId queueHost=$queueHost

