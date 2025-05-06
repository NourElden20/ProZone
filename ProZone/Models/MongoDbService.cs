using MongoDB.Driver;
using MongoDB.Driver.Core;

namespace ProZone.Models
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        public MongoDbService(IConfiguration configuration)
        {
            var mongoDbConfig = configuration.GetSection(nameof(MongoDbConfig)).Get<MongoDbConfig>();

            // Use the connection string from configuration (you don't need to manually append the database name here)
            var mongoClient = new MongoClient(mongoDbConfig.ConnectionString);

            // Use the configured database name to get the database
            _database = mongoClient.GetDatabase(mongoDbConfig.Name);
        }

        public IMongoDatabase Database => _database;

        internal IMongoDatabase GetDatabase()
        {
            return _database;
        }
    }
}
