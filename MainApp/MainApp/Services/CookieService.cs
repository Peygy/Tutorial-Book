using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using MainApp.Models;
using System.Data;

namespace MainApp.Services
{
    // Service to generate cookies when logging in or registering
    // and deleting them when logging out of the site
    public class CookieService : ICookieService
    {
        // Logger for exceptions
        private readonly ILogger<CookieService> log;

        public CookieService(ILogger<CookieService> log)
        {
            this.log = log;
        }



        // HttpContext - for cookies & sessions
        public async Task CookieAuthenticateAsync(string login, string role, HttpContext httpcontext)
        {
            try
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, login),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, role)
                };

                await httpcontext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                await httpcontext.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies")));
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            } 
        }

        public void SessionAuthenticate(string login, string role, HttpContext httpcontext)
        {
            try
            {
                httpcontext.Session.SetString("login", login);
                httpcontext.Session.SetString("role", role);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }     
        }


        public async Task LogoutAsync(HttpContext httpcontext)
        {
            try
            {
                await httpcontext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                httpcontext.Session.Clear();
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }
        }


        public User GetUserInfo(HttpContext httpcontext)
        {
            try
            {
                var loginCookie = httpcontext.User.FindFirst(ClaimsIdentity.DefaultNameClaimType);
                var roleCookie = httpcontext.User.FindFirst(ClaimsIdentity.DefaultRoleClaimType);
                if (loginCookie != null && roleCookie != null)
                {
                    return new User { Login = loginCookie.Value, Role = roleCookie.Value };
                }

                var loginSession = httpcontext.Session.GetString("login");
                var roleSession = httpcontext.Session.GetString("role");
                if (loginSession != null && roleSession != null)
                {
                    return new User { Login = loginSession, Role = roleSession };
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }

            return new User { Role = "null" };
        }
    }
}
