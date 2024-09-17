namespace CLAPi.Core.Settings;

public class MailCredential
{
	private MailCredential()
	{

	}
	private static readonly Lazy<MailCredential> instance = new(() => new MailCredential());
	public static MailCredential Instance => instance.Value;
	public static string SmtpServer { get; set; } = null!;
	public static int SmtpPortNo { get; set; }
	public static string MailAddress { get; set; } = null!;
	public static string MailUserName { get; set; } = null!;
	public static string MailPassword { get; set; } = null!;
	public static string BaseAddress { get; set; } = null!;
	public static string EmailSource { get; set; } = null!;
}
