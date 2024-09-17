using CLAPi.Core.FileStorage;

namespace ConfigurationUtility.Storage;

public interface IFileService
{
    FileStorageInfoObj UploadFile(MemoryStream stream, FileStorageInfo info);
    string GetFileUrl(FileStorageInfo info);
    byte[] GetFileStream(FileStorageInfo info);
}
