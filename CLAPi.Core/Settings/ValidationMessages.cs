namespace CLAPi.Core.Settings;

public sealed class ValidationMessages
{
    private readonly static Lazy<ValidationMessages> instance = new(() => new ValidationMessages());
    public static ValidationMessages Instance => instance.Value;
    public static List<CodeMessage> CodeMessages { get; set; } = null!;
    private ValidationMessages()
    {

    }
}

public class CodeMessage
{
    public int Code { get; set; }
    public string Message { get; set; } = null!;
}
