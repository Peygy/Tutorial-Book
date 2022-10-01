namespace MainApp.Models
{
    public interface IAuthService
    {
        // Checking the user for uniqueness in the database
        bool AvailabilityCheck(string userLogin);

        // Adding new user
        Task AddUserAsync(CreateUserModel newUser);

        // Users & Crew Authentications methods
        Task<bool> UserAuthenticationAsync(User user);
        Task<bool> CrewAuthenticationAsync(Admin admin, string role);
    }
}
