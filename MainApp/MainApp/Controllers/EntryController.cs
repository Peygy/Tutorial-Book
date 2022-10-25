using Microsoft.AspNetCore.Mvc;
using MainApp.Services;
using MainApp.Models;

namespace MainApp.Controllers
{
    // Controller for managing user and staff registrations and logins
    public class EntryController : Controller
    {
        private IAuthService authService;
        private ICookieService cookieService;

        public EntryController(UserContext userData, ILogger<AuthService> authLog, ILogger<CookieService> cookieLog)
        {
            // Data context for parts -> userData
            // Logger for exceptions -> authLog & cookieLog
            // HttpContext for cookies & sessions
            authService = new AuthService(userData, authLog);
            cookieService = new CookieService(cookieLog);
        }



        // Account registration
        [HttpGet]
        public IActionResult UserRegistration()
            => View();

        [HttpPost]
        public async Task<IActionResult> UserRegistration(CreateUserModel newUser)
        {
            if (ModelState.IsValid)
            {
                if (authService.AvailabilityCheck(newUser.Login))
                {
                    await cookieService.CookieAuthenticateAsync(newUser.Login, "user", HttpContext);
                    await authService.AddUserAsync(newUser);
                    return RedirectToAction("Study", "Page");
                }

                ViewBag.Error = "Пользователь с таким логином уже существует";
                return View(newUser);
            }

            return View(newUser);
        }



        // Account login
        [HttpGet]
        public IActionResult UserLogin()
        {
            if (cookieService.GetUserInfo(HttpContext).Login != null)
                return RedirectToAction("Study", "Page");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UserLogin(User user, bool remember)
        {
            if (ModelState.IsValid)
            {
                if (!await authService.UserAuthenticationAsync(user))
                {
                    ViewBag.Error = "Логин или пароль неверны!";
                    return View(user);
                }

                if (remember)
                    await cookieService.CookieAuthenticateAsync(user.Login, "user", HttpContext);
                else
                    cookieService.SessionAuthenticate(user.Login, "user", HttpContext);

                return RedirectToAction("Study", "Page");
            }

            return View(user);
        }



        // Account logout
        public async Task<IActionResult> Logout()
        {
            await cookieService.LogoutAsync(HttpContext);
            return RedirectToAction("Welcome", "Page");
        }



        // Crew login
        [HttpGet]
        public IActionResult CrewLogin()
        {
            switch (cookieService.GetUserInfo(HttpContext).Role) 
            {
                case "admin" or "editor": return RedirectToAction("ViewParts", "Part", new { table = "sections" });
                case "user": return RedirectToAction("Study", "Page");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CrewLogin(Admin admin)
        {
            if (ModelState.IsValid)
            {
                if(await authService.CrewAuthenticationAsync(admin, "admin"))
                {
                    await cookieService.CookieAuthenticateAsync(admin.Login, "admin", HttpContext);
                    return RedirectToAction("ViewParts", "Part", new { table = "sections" });
                }
                else if (await authService.CrewAuthenticationAsync(admin, "editor"))
                {
                    await cookieService.CookieAuthenticateAsync(admin.Login, "editor", HttpContext);
                    return RedirectToAction("ViewParts", "Part", new { table = "sections" });
                }

                ViewBag.Error = "Логин или пароль неверны!";
                return View(admin);
            }

            return View(admin);
        }
    }
}
