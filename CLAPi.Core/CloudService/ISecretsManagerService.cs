using CLAPi.Core.Settings;

namespace CLAPi.Core.CloudService;

public interface ISecretsManagerService
{
    SecretValueResponse? GetSecretManagers(string secretManagerName);
}
