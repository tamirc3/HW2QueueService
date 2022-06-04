namespace AutoScaleService.Services;

public interface IAppServiceManager
{
    void CreateInfraResources();
    Task CreateAppServiceAsync();
    Task DeleteAppService();
}