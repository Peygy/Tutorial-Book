using Microsoft.EntityFrameworkCore;
using MainApp.Models;

namespace MainApp.Services
{
    public class AuthService : IAuthService
    {
        // Data context for users and crew
        private readonly UserContext userData;
        // Logger for exceptions
        private readonly ILogger<AuthService> log;

        public AuthService(UserContext userData, ILogger<AuthService> log)
        {
            this.userData = userData;
            this.log = log;
        }



        public bool AvailabilityCheck(string userLogin)
        {
            try
            {
                if (!userData.Users.Any(u => u.Login == userLogin)) return true;
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }

            return false;
        }



        public async Task AddUserAsync(CreateUserModel newUser)
        {
            try
            {
                string newPassword = HashService.HashPassword(newUser.Password);      
                await userData.Users.AddAsync(
                    new User { Login = newUser.Login, Password = newPassword });
                await userData.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }
        }



        public async Task<bool> UserAuthenticationAsync(User user)
        {
            try
            {
                if (userData.Users.Any(u => u.Login == user.Login))
                {
                    var userDb = await userData.Users.FirstOrDefaultAsync(u => u.Login == user.Login);
                    if (HashService.VerifyHashedPassword(userDb.Password, user.Password)) return true;
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }

            return false;
        }


        
        public async Task<bool> CrewAuthenticationAsync(Admin admin, string role)
        {
            try
            {
                if (userData.Crew.Any(c => c.Login == admin.Login && c.Role == role))
                {
                    var admDb = await userData.Crew.FirstOrDefaultAsync(c => c.Login == admin.Login);
                    if (HashService.VerifyHashedPassword(admDb.Password, admin.Password)) 
                    { return true; }                 
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }

            return false;
        }
    }
}
