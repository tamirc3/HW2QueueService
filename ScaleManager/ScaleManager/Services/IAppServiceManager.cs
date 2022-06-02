namespace AutoScaleService.Services;

public interface IAppServiceManager
{
    void CreateInfraResources();
    void CreateAppService();
    void DeleteAppService();
}