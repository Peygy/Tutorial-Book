using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MainApp.Models;

namespace MainApp.Services
{
    public class MongoService : IMongoService
    {
        // Collection in db
        private readonly IMongoCollection<ContentModel> contentsCollection;
        // Logger for exceptions
        private readonly ILogger<IMongoService> log;

        public MongoService(IOptions<ContentDataSettings> options, ILogger<IMongoService> log)
        {
            this.log = log;

            var client = new MongoClient(options.Value.ConnectionString);
            var database = client.GetDatabase(options.Value.DatabaseName);
            contentsCollection = database.GetCollection<ContentModel>(options.Value.ContentCollectionName);
        }


        public async Task<string> GetContentAsync(string? id)
        {
            try
            {
                return (await contentsCollection.Find(new BsonDocument("_id",
                    new ObjectId(id))).FirstOrDefaultAsync()).Content;
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }

            return string.Empty;
        }

        public async Task<string?> AddContentAsync(ContentModel obj)
        {
            try
            {
                await contentsCollection.InsertOneAsync(obj);
                return obj.Id;
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }

            return string.Empty;
        }

        public async Task UpdateContentAsync(ContentModel obj)
        {  
            try
            {
                await contentsCollection.ReplaceOneAsync(new BsonDocument("_id",
                    new ObjectId(obj.Id)), obj);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }
        }

        public async Task RemoveContentAsync(string? id)
        {
            try
            {
                await contentsCollection.DeleteOneAsync(new BsonDocument("_id",
                    new ObjectId(id)));
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }
        }
    }
}
