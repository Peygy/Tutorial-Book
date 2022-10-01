using System.ComponentModel.DataAnnotations;

namespace MainApp.Models
{
    // User model
    public class User
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Не указан логин!")]
        public string Login { get; set; } = null!;
        [Required(ErrorMessage = "Не указан пароль!")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
        // Role for authorization 
        public string Role { get; set; } = "user";
    }


    // Admin model
    sealed public class Admin
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Не указан логин")]
        public string Login { get; set; } = null!;
        [Required(ErrorMessage = "Не указан пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
        // Role for authorization
        public string Role { get; set; } = "admin";
    }


    // Model for user registration
    sealed public class CreateUserModel
    {
        [Required(ErrorMessage = "Не указан логин!")]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "Длина логина должна быть от 4 до 20 символов")]
        public string Login { get; set; } = null!;
        [Required(ErrorMessage = "Не указан пароль!")]
        [StringLength(20, MinimumLength = 7, ErrorMessage = "Длина пароля должна быть от 7 до 20 символов")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
        [Required(ErrorMessage = "Не подтвержден пароль!")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают!")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = null!;
    }
}