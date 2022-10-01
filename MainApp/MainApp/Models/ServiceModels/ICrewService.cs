namespace MainApp.Models
{
    // Service for fetching users or crew from the database and passing them to display
    public interface ICrewService
    {
        // Getting data about users and crew
        Task<IEnumerable<User>?> GetUsersAsync();
        Task<IEnumerable<Admin>?> GetAdminsAsync();

        // Adding data
        Task<bool> AddAdminAsync(Admin newAdmin);

        // Deleting data
        Task<User?> RemoveUserAsync(int id);
        Task<Admin?> RemoveAdminAsync(int id);
    }
}
