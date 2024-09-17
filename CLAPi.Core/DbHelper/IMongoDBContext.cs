namespace CLAPi.Core.DBHelper;

public interface IMongoDBContext
{
    Task<List<T>> GetAllAsync<T>(string collectionName);
    Task<List<R>> GetAsync<R>(Dictionary<string, object> filter, string collectionName) where R : class;
    Task InsertAsync<T>(T request, string collectionName);
    Task UpdateAsync<T>(T request, string collectionName);
}
