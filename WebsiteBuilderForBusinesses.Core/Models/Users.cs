using WebsiteBuilderForBusinesses.Core.Abstractions;
using WebsiteBuilderForBusinesses.Core.Infrastructures;
using WebsiteBuilderForBusinesses.Core.Services;

namespace WebsiteBuilderForBusinesses.Core.Models
{
    public class Users
    {
        public Guid Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string HashPassword { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public static IPasswordHasherService PasswordHasherService { get; set; } 
            = new PasswordHasherService();


        private Users(Guid id, string login, string hashPassword, string role,
            IPasswordHasherService passwordHasherService)
        {
            Id = id;
            Login = login;
            HashPassword = hashPassword;
            Role = role;
            PasswordHasherService = passwordHasherService;
        }
        public static bool VerifyPassword(string password, string hashPassword)
        {
            return PasswordHasherService.VerifyBCrypt(password, hashPassword);
        }

        public static ResultModel<Users> Create(Guid id, string login, string password,
            string role, IPasswordHasherService passwordHasherService)
        {
            if (id == Guid.Empty)
                return ResultModel<Users>.Failure("Поле Id не должно быть пустым");

            if (string.IsNullOrWhiteSpace(login))
                return ResultModel<Users>.Failure("Поле Имя не должно быть пустым");

            if (string.IsNullOrWhiteSpace(password))
                return ResultModel<Users>.Failure("Поле Пароль не должно быть пустым");

            if (string.IsNullOrWhiteSpace(role))
                return ResultModel<Users>.Failure("Поле Роль не должно быть пустым");

            return ResultModel<Users>.Success(new Users(id, login,
                passwordHasherService.HashBCrypt(password), role, passwordHasherService));
        }
    }
}
