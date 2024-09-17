namespace CLAPi.Core.Settings;

public class MongoDbSettings
{
    private static readonly Lazy<MongoDbSettings> instance = new(() => new MongoDbSettings());
    public static MongoDbSettings Instance => instance.Value;
    public static string ConnectionString { get; set; } = null!;
    public static string DatabaseName { get; set; } = null!;
    private MongoDbSettings()
    {
        
    }
}
