namespace CLAPi.Core.Settings;

public class SmsCredential
{
    private SmsCredential()
    {

    }
    private static readonly Lazy<SmsCredential> instance = new(() => new SmsCredential());
    public static SmsCredential Instance => instance.Value;
    public static string BaseAddress { get; set; } = null!;
    public static string? SignName { get; set; }
    public static string OTPTemplateCode { get; set; } = null!;
}