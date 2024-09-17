namespace CLAPi.Core.Settings;

public class AwsSettings
{
    private static readonly Lazy<AwsSettings> instance = new(() => new AwsSettings());
    public static AwsSettings Instance => instance.Value;
    private AwsSettings()
    {

    }
    public static string? Profile { get; set; }
    public static string? Region { get; set; }
    public static string? RoleArn { get; set; }
    public static string? RoleSessionName { get; set; }
    public static string? AWSAccessKey { get; set; }
    public static string? AWSSecreKey { get; set; }
    public static AwsS3BucketOptions AwsS3BucketOptions { get; set; } = null!;
}
public class AwsS3BucketOptions
{
    private static readonly Lazy<AwsS3BucketOptions> instance = new(() => new AwsS3BucketOptions());
    public static AwsS3BucketOptions Instance => instance.Value;
    public static string BucketName { get; set; } = null!;
}
