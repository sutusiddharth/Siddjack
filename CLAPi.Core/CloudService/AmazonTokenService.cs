using Amazon;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using CLAPi.Core.Settings;

namespace CLAPi.Core.CloudService;

public abstract class AmazonTokenService
{
    protected static Credentials GetCredentials()
    {
        AssumeRoleRequest request = new()
        {
            RoleArn = AwsSettings.RoleArn,
            RoleSessionName = AwsSettings.RoleSessionName
        };
        return new AmazonSecurityTokenServiceClient().AssumeRoleAsync(request).Result.Credentials;
    }

    protected static RegionEndpoint GetRegion()
    {
        return RegionEndpoint.GetBySystemName(AwsSettings.Region);
    }
}