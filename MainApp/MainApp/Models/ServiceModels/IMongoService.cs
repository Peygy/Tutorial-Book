namespace MainApp.Models
{
    public interface IMongoService
    {
        Task<string> GetContentAsync(string? id);

        Task<string?> AddContentAsync(ContentModel obj);

        Task UpdateContentAsync(ContentModel obj);

        Task RemoveContentAsync(string? id);
    }
}
