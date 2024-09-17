namespace CLAPi.Core.Settings;

public class AppSettings
{
    private static readonly Lazy<AppSettings> instance = new(() => new AppSettings());
    public static AppSettings Instance => instance.Value;
    private AppSettings()
    {

    }
    public static string? DefaultDate { get; set; }
    public static string ContentRootPath { get; set; } = null!;
    public static string EnvironmentName { get; set; } = null!;
    public static string WebRootPath { get; set; } = null!;
}
