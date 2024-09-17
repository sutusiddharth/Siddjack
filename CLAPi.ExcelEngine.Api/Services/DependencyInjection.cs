using CLAPi.Core.DBHelper;
using CLAPi.Core.FileStorage;
using CLAPi.ExcelEngine.Api.BackGroundJob;
using ConfigurationUtility.Storage;
using Microsoft.Extensions.Caching.Memory;

namespace CLAPi.ExcelEngine.Api.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
    {
        services.AddSingleton<IMemoryCache, MemoryCache>();
        services.AddSingleton<IMongoDBContext, MongoDBContext>();
        services.AddScoped<IFileService, AwsService>();
        services.AddScoped<ITokenDetail, TokenDetail>();
        services.AddSingleton<IBackgroundTaskQueue>(new BackgroundTaskQueue(100));
        ////services.AddHttpClient<ApiService>();
        ////services.AddSingleton<ISecretsManagerService, AwsSecretsManagerService>();
        return services;
    }
}