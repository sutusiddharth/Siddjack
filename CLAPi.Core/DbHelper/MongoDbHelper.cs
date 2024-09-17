using CLAPi.Core.Settings;
using MongoDB.Driver;

namespace CLAPi.Core.DBHelper;

public class MongoDBContext : IMongoDBContext
{
    private readonly IMongoDatabase _database;

    public MongoDBContext()
    {
        var connectionString = MongoDbSettings.ConnectionString;
        var databaseName = MongoDbSettings.DatabaseName;

        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }
    public async Task<List<T>> GetAllAsync<T>(string collectionName)
    {
        var _collection = _database.GetCollection<T>(collectionName);

        var result = await _collection.FindAsync(Builders<T>.Filter.Empty);
        return await result.ToListAsync();

    }
    public async Task<List<R>> GetAsync<R>(Dictionary<string, object> filter, string collectionName) where R : class
    {
        var _collection = _database.GetCollection<R>(collectionName);

        var builder = Builders<R>.Filter;
        List<FilterDefinition<R>> filterDefinition = [];

        foreach (var property in filter)
        {
            filterDefinition.Add(builder.Eq(property.Key, property.Value));
        }
        var filteration = builder.And(filterDefinition);
        var result = await _collection.FindAsync(filteration);
        return await result.ToListAsync();

    }
    public async Task InsertAsync<T>(T request, string collectionName)
    {
        var _collection = _database.GetCollection<T>(collectionName);


        await _collection.InsertOneAsync(request);

    }
    public async Task UpdateAsync<T>(T request, string collectionName)
    {
        var _collection = _database.GetCollection<T>(collectionName);

        var builder = Builders<T>.Filter;
        List<FilterDefinition<T>> filterDefinition = [];
        var Id = request?.GetType().GetProperty("Id");

        // Add filter based on id parameter
        filterDefinition.Add(builder.Eq(Id?.Name, Id?.GetValue(request)));

        var filteration = builder.And(filterDefinition);

        await _collection.ReplaceOneAsync(filteration, request);

    }
}