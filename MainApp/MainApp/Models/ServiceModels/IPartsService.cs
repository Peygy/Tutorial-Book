namespace MainApp.Models
{
    // Service for fetching parts from the database and passing them to display
    public interface IPartsService
    {
        // Getting data about parts
        Task<PartModel?> GetPartAsync(int partId, string table, string? title, string? filtre);
        // Filtering children
        Task<IEnumerable<T>?> ChildrenFilter<T>
        (IEnumerable<T>? children, string table, string? title, string? filtre) where T : PartModel;
        Task<IEnumerable<PartModel>> GetAllParentsAsync(string table);
        Task<string> GetPostContentAsync(string? contentId);

        // Adding data
        Task<bool> AddPartAsync(PartModel newPart, string content);

        // Updating data
        Task<bool> UpdatePartAsync(PartModel part, string newContent);

        // Deleting data
        Task<int?> RemovePartAsync(int partId, string table);

        // Other tools
        Task<bool> CheckTitleExistanceAsync(int partId, string parentTable, string title);
    }
}
