namespace MainApp.Models
{
    public interface ICookieService
    {
        Task CookieAuthenticateAsync(string login, string role, HttpContext httpcontext);

        void SessionAuthenticate(string login, string role, HttpContext httpcontext);

        Task LogoutAsync(HttpContext httpcontext);

        User GetUserInfo(HttpContext httpcontext);
    }
}
