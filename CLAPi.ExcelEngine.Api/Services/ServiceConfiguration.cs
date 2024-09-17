using CLAPi.Core.Settings;

namespace CLAPi.ExcelEngine.Api.Services;

public static class ServiceConfiguration
{
    public static void AddConfig(this WebApplicationBuilder builder)
    {
        ConfigurationBuilder configurationBuilder = new();
        configurationBuilder.AddJsonFile("appsettings.json");
        builder.Configuration.GetSection("AppSettings").Bind(AppSettings.Instance);
        builder.Configuration.GetSection("AwsSettings").Bind(AwsSettings.Instance);
        builder.Configuration.GetSection("MongoDbSettings").Bind(MongoDbSettings.Instance);
        builder.Configuration.GetSection("SmsCredential").Bind(SmsCredential.Instance);
        builder.Configuration.GetSection("MailCredential").Bind(MailCredential.Instance);
        builder.Configuration.GetSection("JwtSettings").Bind(JwtSettings.Instance);

        AppSettings.ContentRootPath = builder.Environment.ContentRootPath;
        AppSettings.EnvironmentName = builder.Environment.EnvironmentName;
        AppSettings.WebRootPath = builder.Environment.WebRootPath;
    }
}