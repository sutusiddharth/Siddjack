namespace CLAPi.Core.Settings;

public class SecretValueResponse
{
    public string EndPoint { get; set; } = null!;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Ip_Address { get; set; } = string.Empty;
    public string Secret_Key { get; set; } = string.Empty;
}
