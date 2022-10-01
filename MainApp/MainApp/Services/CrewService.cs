using Microsoft.EntityFrameworkCore;
using MainApp.Models;

namespace MainApp.Services
{
    // Service for fetching users or crew from the database and passing them to display
    public class CrewService : ICrewService
    {
        // Data context for users and crew
        private readonly UserContext userData;
        // Logger for exceptions
        private readonly ILogger<CrewService> log;

        public CrewService(UserContext userData, ILogger<CrewService> log)
        {
            this.userData = userData;
            this.log = log;
        }



        public async Task<IEnumerable<User>?> GetUsersAsync()
        {
            try
            {
                return await userData.Users.ToListAsync();
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }

            return null;
        }

        public async Task<IEnumerable<Admin>?> GetAdminsAsync()
        {
            try
            {
                return await userData.Crew.Where(a => a.Role == "admin").ToListAsync();
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }

            return null;
        }



        public async Task<bool> AddAdminAsync(Admin newAdmin)
        {
            try
            {
                if (!userData.Crew.Any(a => a.Login == newAdmin.Login))
                {
                    newAdmin.Password = HashService.HashPassword(newAdmin.Password);
                    await userData.Crew.AddAsync(newAdmin);
                    await userData.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }

            return false;
        }



        public async Task<User?> RemoveUserAsync(int id)
        {
            try
            {
                var user = await userData.Users.FirstOrDefaultAsync(u => u.Id == id);
                userData.Users.Remove(user);
                await userData.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }

            return null;
        }

        public async Task<Admin?> RemoveAdminAsync(int id)
        {
            try
            {
                var admin = await userData.Crew.FirstOrDefaultAsync(u => u.Id == id);
                userData.Crew.Remove(admin);
                await userData.SaveChangesAsync();
                return admin;
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }

            return null;
        }
    }
}
