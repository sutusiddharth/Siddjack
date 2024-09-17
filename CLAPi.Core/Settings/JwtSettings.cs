namespace CLAPi.Core.Settings;

public class JwtSettings
{
    private static readonly Lazy<JwtSettings> instance = new(() => new JwtSettings());
    public static JwtSettings Instance => instance.Value;
    private JwtSettings()
    {

    }
    public static string Issuer { get; set; } = null!;
    public static string Audience { get; set; } = null!;
    public static string SecretKey { get; set; } = null!;
    public static string AccessTokenExpirationMinutes { get; set; } = null!;
    public static string RefreshTokenExpirationDays { get; set; } = null!;
}
