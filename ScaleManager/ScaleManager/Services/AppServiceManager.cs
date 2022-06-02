using System.Collections.Concurrent;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace AutoScaleService.Services;

public class AppServiceManager : IAppServiceManager
{
    private IAzure _azureConnection;
    private readonly Region _region = Region.USEast;
    private readonly Dictionary<string, string> _tags = new() { { "environment", "development" } };
    private IResourceGroup? _resourceGroup;
    private IAppServicePlan? _appServicePlan;
    private readonly ConcurrentQueue<IWebApp> appSerivcePool = new ConcurrentQueue<IWebApp>();


    //from https://github.com/Azure-Samples/app-service-dotnet-configure-deployment-sources-for-web-apps/blob/master/Program.cs
    public void CreateInfraResources()
    {

        //todo, in a script create a servicePrinicpal, get its secreat and put it in the app config to use
        //MPDP-test-deleteME
        var creds = SdkContext.AzureCredentialsFactory.FromServicePrincipal(
            "sp ID",
            "secreate",
            "tenant",
            AzureEnvironment.AzureGlobalCloud);

        _azureConnection = Azure.Configure()
            .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
            .Authenticate(creds).WithDefaultSubscription();

        CreateResourceGroup();
        CreateAsp();

    }

    //todo remove it since we are creating the first one via the script
    private void CreateResourceGroup()
    {
        _resourceGroup = _azureConnection.ResourceGroups
            .Define("tacaspi-dotnet2-eus-rg")
            .WithRegion(_region)
            .WithTags(_tags)
            .Create();
    }

    private void CreateAsp()
    {
        string appServiceName ="ASP_" + _region.Name + "_" + Guid.NewGuid();
        _appServicePlan = _azureConnection.AppServices.AppServicePlans
            .Define(appServiceName)
            .WithRegion(_region)
            .WithExistingResourceGroup(_resourceGroup)
            .WithFreePricingTier()
            .WithTags(_tags)
            .Create();
    }

    public void CreateAppService()
    {
        string appServiceName = "App_" + _region.Name + "_" + Guid.NewGuid();

        var app = _azureConnection.WebApps
            .Define(appServiceName)
            .WithExistingWindowsPlan(_appServicePlan)
            .WithExistingResourceGroup(_resourceGroup)
            .DefineSourceControl()
            .WithPublicGitRepository("https://github.com/tamirc3/parkinglotCloud")
            .WithBranch("main")
            .Attach()
            .Create();
            
        appSerivcePool.Enqueue(app);
    }

    public void DeleteAppService()
    {
        appSerivcePool.TryDequeue(out var webApp);

        //todo check if the webapp is busy before deleting it

        if (webApp != null) _azureConnection.WebApps.DeleteById(webApp.Name);
    }

    public int GetNumberOFWorkers()
    {
        return appSerivcePool.Count;
    }
}