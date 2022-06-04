az login --use-device-code

# # see which avilable locations we have
$locations = az account list-locations | ConvertFrom-Json
$choosenLocation =  $locations[0].name
echo $choosenLocation

$randomIdentifier= "1" #Get-Random // can add this flag to generate a unique name each deployment
$location=$choosenLocation
$resourceGroup="HomeWork2-cloudcomputing-rg"
$tag="deploy-github"
$gitrepoQueue="https://github.com/evyatarweiss/CloudComputinghw2_QueueApi" 
$appServicePlan="parkinglot-app-service-plan-$randomIdentifier"
$webappQueue="hw2-web-app-queue-$randomIdentifier"


# Create a resource group.
echo "Creating $resourceGroup in "$location"..."
az group create --name $resourceGroup --location "$location" --tag $tag

# Create an App Service plan in `FREE` tier.
echo "Creating $appServicePlan"
az appservice plan create --name $appServicePlan --resource-group $resourceGroup --sku FREE

# Create a web app for queue.
echo "Creating $webappQueue"
$QueueAPIwebApp = az webapp create --name $webappQueue --resource-group $resourceGroup --plan $appServicePlan | ConvertFrom-Json


# Deploy the Queue code from a public GitHub repository. 
az webapp deployment source config --name $webappQueue --resource-group $resourceGroup `
--repo-url $gitrepoQueue --branch main --manual-integration



echo $QueueAPIwebApp.defaultHostName

$webappAPI="hw2-web-app-API-$randomIdentifier"
# Create a web app for API.

echo "Creating $webappAPI"
az webapp create --name $webappAPI --resource-group $resourceGroup --plan $appServicePlan 

$gitrepoAPI="https://github.com/evyatarweiss/-CloudComputinghw2_QueueServiceApi" 

#Deploy webApp API
az webapp deployment source config --name $webappAPI --resource-group $resourceGroup `
--repo-url $gitrepoAPI --branch main --manual-integration 

################################### Changing the API appsettings.json ###############

Connect-AzureRmAccount
$webAppSettings = Get-AzureRmWebApp -ResourceGroupName $resourceGroup -Name $webappAPI
$appsettingList=$webAppSettings.SiteConfig.AppSettings

#current appsettings
$appsettingList

$appsettings = @{}
ForEach ($k in $appsettingList) {
    $appsettings[$k.Name] = $k.Value
}

#modify the settings
$appsettings['QueueHost'] = "https://" + $QueueAPIwebApp.defaultHostName

#save appsettings
set-AzureRmWebApp -resourcegroupname $resourceGroup -name $webappAPI -appsettings $appsettings

############ Worker node web app ###############

$webappWN="hw2-web-app-WorkerNode-$randomIdentifier"
# Create a web app for API.

echo "Creating $webappWN"
az webapp create --name $webappWN --resource-group $resourceGroup --plan $appServicePlan 

$gitrepoWN="https://github.com/evyatarweiss/CloudComputinghw2_WorkerNode" 

#Deploy webApp API
az webapp deployment source config --name $webappWN --resource-group $resourceGroup `
--repo-url $gitrepoWN --branch main --manual-integration 

## Changed appsetting.json for worker node
set-AzureRmWebApp -resourcegroupname $resourceGroup -name $webappWN -appsettings $appsettings


############### 