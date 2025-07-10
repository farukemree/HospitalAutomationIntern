using BCrypt.Net;
using HospitalAutomation.DataAccess.Context;
using HospitalAutomation.DataAccess.DTOs;
using HospitalAutomation.DataAccess.Models;
using HospitalAutomation.DataAccess.Repositories;
using HospitalAutomation.Service.Interfaces;
using HospitalAutomation.Service.Response;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HospitalAutomation.Service.Services
{
    public class UserService:IUserService
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public UserService(IGenericRepository<User> userRepository, IConfiguration configuration, AppDbContext context)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _context = context;

        }


        public ResponseGeneric<string> Login(UserLoginDto user)
        {
            if (user == null)
                return ResponseGeneric<string>.Error("Kullanıcı bilgileri boş olamaz.");

            if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password))
                return ResponseGeneric<string>.Error("Kullanıcı adı ve şifre boş olamaz.");

            try
            {
               
                var usernameParam = new SqlParameter("@Username", user.Username);

               
                var existingUser = _context.Users
                    .FromSqlRaw("EXEC Pr_Get_User_By_Username @Username", usernameParam)
                    .AsEnumerable() // EF Core'dan normal C# enumerable'a geçiş
                    .FirstOrDefault();

               
                if (existingUser == null)
                    return ResponseGeneric<string>.Error("Kullanıcı adı veya şifre yanlış.");

               
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(user.Password, existingUser.Password);
                if (!isPasswordValid)
                    return ResponseGeneric<string>.Error("Kullanıcı adı veya şifre yanlış.");

                var generatedToken = GenerateJwtToken(existingUser);

                return ResponseGeneric<string>.Success(generatedToken, "Giriş başarılı.");
            }
            catch (Exception ex)
            {
                return ResponseGeneric<string>.Error("Giriş sırasında hata oluştu: " + ex.Message);
            }
        }




        public ResponseGeneric<bool> Register(UserRegisterDto userDto)
        {
            try
            {
               
                var existingUser = _userRepository.Get(u => u.Username == userDto.Username);
                if (existingUser != null)
                {
                    return ResponseGeneric<bool>.Error("Bu kullanıcı adı zaten mevcut.");
                }

               
                var hashedPassword = HashPassword(userDto.Password);

               
                var user = new User
                {
                    Username = userDto.Username,
                    Password = hashedPassword,
                    Email = userDto.Email,
                    Role = "Patient"
                };


                _context.Database.ExecuteSqlRaw(
            "EXEC Pr_Create_User @Username, @Password, @Email, @Role",
            new SqlParameter("@Username", user.Username),
            new SqlParameter("@Password", user.Password),
            new SqlParameter("@Email", user.Email),
            new SqlParameter("@Role", user.Role)
        );

                return ResponseGeneric<bool>.Success(true, "Kayıt başarılı.");
            }
            catch (Exception ex)
            {
                return ResponseGeneric<bool>.Error("Kayıt sırasında hata oluştu: " + ex.Message);
            }
        }


        private string HashPassword(string password)
        {

            string secretKey = "*ZCkiEre7EbhJ_k";
            using (var sha256 = SHA256.Create())
            {
                var combinedPassword = password + secretKey;
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedPassword));
                var hashedPassword = Convert.ToBase64String(bytes);
                return hashedPassword;
            }
           
        }
        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };
            if (!string.IsNullOrEmpty(user.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, user.Email));
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );   


            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public ResponseGeneric<bool> UpdateUserRole(string username, string role)
        {
            try
            {
                var sql = "EXEC Pr_Update_User_Role @Username, @Role";

                var parameters = new[]
                {
            new SqlParameter("@Username", username),
            new SqlParameter("@Role", role)
        };

                _context.Database.ExecuteSqlRaw(sql, parameters);

                return ResponseGeneric<bool>.Success(true, "Rol güncellendi.");
            }
            catch (Exception ex)
            {
                return ResponseGeneric<bool>.Error("Hata oluştu: " + ex.Message);
            }
        }


    }
}

