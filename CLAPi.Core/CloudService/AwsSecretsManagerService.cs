using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using CLAPi.Core.Settings;
using System.Text.Json;

namespace CLAPi.Core.CloudService;

public class AwsSecretsManagerService : AmazonTokenService, ISecretsManagerService
{
	private static readonly JsonSerializerOptions CamelCaseOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};
	public SecretValueResponse? GetSecretManagers(string secretManagerName)
	{
		AmazonSecretsManagerClient client = new(GetCredentials(), GetRegion());
		var request = new GetSecretValueRequest
		{
			SecretId = Environment.GetEnvironmentVariable(secretManagerName)
		};
		var secretStringJson = client.GetSecretValueAsync(request).Result.SecretString;
		return JsonSerializer.Deserialize<SecretValueResponse>(secretStringJson, CamelCaseOptions);
	}
}
