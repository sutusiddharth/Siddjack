using System.Net;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using CLAPi.Core.GenericServices;
using CLAPi.Core.Settings;
using ConfigurationUtility.Storage;
using Microsoft.Extensions.Hosting;

namespace CLAPi.Core.FileStorage;

public class AwsService : IFileService
{
    private AmazonS3Client _s3Client = null!;
    public FileStorageInfoObj UploadFile(MemoryStream stream, FileStorageInfo info)
    {
        GetSecurityToken();
        FileStorageInfoObj file = new();
        PutObjectRequest request = new()
        {
            CannedACL = S3CannedACL.Private,
            InputStream = stream,
            BucketName = AwsS3BucketOptions.BucketName,
            Key = $"{info.Folder_Path}/{info.File_Nm}"
        };

        var response = _s3Client.PutObjectAsync(request).Result;

        if (response.HttpStatusCode == HttpStatusCode.OK)
        {
            GetPreSignedUrlRequest getPreSigned = new()
            {
                BucketName = request.BucketName,
                Key = request.Key,
                Expires = DateTime.Now.AddHours(ConstantValues.Hour)
            };
            file = new FileStorageInfoObj() { Url = _s3Client.GetPreSignedURL(getPreSigned) };
        }
        return file;
    }
    public string GetFileUrl(FileStorageInfo info)
    {
        GetSecurityToken();
        GetPreSignedUrlRequest getPreSigned = new()
        {
            BucketName = AwsS3BucketOptions.BucketName,
            Key = $"{info.Folder_Path}/{info.File_Nm}",
            Expires = DateTime.Now.AddHours(ConstantValues.Hour)
        };
        var url = _s3Client.GetPreSignedURL(getPreSigned);

        return url;
    }
    public byte[] GetFileStream(FileStorageInfo info)
    {
        GetSecurityToken();
        byte[] array = null!;
        GetObjectRequest request = new()
        {
            BucketName = AwsS3BucketOptions.BucketName,
            Key = $"{info.Folder_Path}/{info.File_Nm}"
        };
        try
        {
            var response = _s3Client.GetObjectAsync(request).Result;

            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                using MemoryStream ms = new();
                response.ResponseStream.CopyTo(ms);
                array = ms.ToArray();
            }
        }
        catch (Exception ex)
        {

            Console.WriteLine(ex.Message);
        }
        finally
        {
            array ??= null!; // Handling Catch Statement returning default
        }

        return array;
    }
    private void GetSecurityToken()
    {
        var client = new AmazonSecurityTokenServiceClient(); //Backup

        try
        {
            if (AppSettings.EnvironmentName == Environments.Development)
            {
                _s3Client = new AmazonS3Client(AwsSettings.AWSAccessKey, AwsSettings.AWSSecreKey, RegionEndpoint.GetBySystemName(AwsSettings.Region));
            }
            else //Release Mode
            {
                AssumeRoleRequest request = new()
                {
                    RoleArn = AwsSettings.RoleArn,
                    RoleSessionName = AwsSettings.RoleSessionName
                };
                var response = client.AssumeRoleAsync(request).Result;  // Backup
                _s3Client = new AmazonS3Client(response.Credentials.AccessKeyId, response.Credentials.SecretAccessKey, response.Credentials.SessionToken, RegionEndpoint.GetBySystemName(AwsSettings.Region));   // Backup
            }
        }
        catch (Exception ex)
        {
            ErrorFormats.ThrowValidationException(ex.Message, nameof(ex.Message));
        }
    }
}