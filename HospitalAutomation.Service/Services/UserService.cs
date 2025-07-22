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
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Data;
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
        private readonly ILogger<UserService> _logger;
        private readonly IPatientService _patientService;

        public UserService(IGenericRepository<User> userRepository, IConfiguration configuration, AppDbContext context, ILogger<UserService> logger, IPatientService patientService)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _context = context;
            _logger = logger;
            _patientService = patientService;

        }
        public ResponseGeneric<List<UserDto>> GetAllUsers()
        {
            try
            {
                var users = _context.Users.ToList();

                if (!users.Any())
                    return ResponseGeneric<List<UserDto>>.Error("Hiç kullanıcı bulunamadı.");

                var userDtos = users.Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role
                }).ToList();

                return ResponseGeneric<List<UserDto>>.Success(userDtos, "Kullanıcılar başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return ResponseGeneric<List<UserDto>>.Error("Veriler alınırken hata oluştu: " + ex.Message);
            }
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
                    .AsEnumerable()
                    .FirstOrDefault();

                if (existingUser == null)
                    return ResponseGeneric<string>.Error("Kullanıcı adı veya şifre yanlış.");

                
                var hashedInputPassword = HashPassword(user.Password);

                if (hashedInputPassword != existingUser.Password)
                    return ResponseGeneric<string>.Error("Kullanıcı adı veya şifre yanlış.");

                var token = GenerateJwtToken(existingUser);

                return ResponseGeneric<string>.Success(token, "Giriş başarılı.");
            }
            catch (Exception ex)
            {
                return ResponseGeneric<string>.Error("Giriş sırasında hata oluştu: " + ex.Message);
            }
        }





        public ResponseGeneric<bool> Register(UserRegisterDto userDto, PatientDto patientDto = null)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var existingUser = _userRepository.Get(u => u.Username == userDto.Username);
                if (existingUser != null)
                    return ResponseGeneric<bool>.Error("Bu kullanıcı adı zaten mevcut.");

                var hashedPassword = HashPassword(userDto.Password);

                // User ekleme sp çağrısı
                var idParam = new SqlParameter
                {
                    ParameterName = "@Id",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Direction = System.Data.ParameterDirection.Output
                };

                _context.Database.ExecuteSqlRaw(
                    "EXEC Pr_Create_User @Id OUTPUT, @Username, @Password, @Email, @Role",
                    idParam,
                    new SqlParameter("@Username", userDto.Username),
                    new SqlParameter("@Password", hashedPassword),
                    new SqlParameter("@Email", userDto.Email),
                    new SqlParameter("@Role", "Patient")
                );

                var userId = (int)idParam.Value;

                if (patientDto != null)
                {
                    _context.Database.ExecuteSqlRaw(
                        "EXEC Pr_Add_Patient @Id, @FullName, @BirthDate, @Gender, @AppointmentDate",
                        new SqlParameter("@Id", userId),
                        new SqlParameter("@FullName", patientDto.FullName),
                        new SqlParameter("@BirthDate", patientDto.BirthDate),
                        new SqlParameter("@Gender", patientDto.Gender ?? (object)DBNull.Value),
                        new SqlParameter("@AppointmentDate", DBNull.Value) // Eğer varsa gerçek değer verilebilir
                    );
                }

                transaction.Commit();

                return ResponseGeneric<bool>.Success(true, "Kayıt başarılı.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
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
        public ResponseGeneric<bool> UpdateUserRole(UpdateUserRoleDto dto)
        {
            try
            {
                var user = _userRepository.Get(u => u.Id == dto.UserId);
                if (user == null)
                    return ResponseGeneric<bool>.Error("Kullanıcı bulunamadı.");

                _context.Database.ExecuteSqlRaw(
                    "EXEC Pr_Update_User_Role @UserId, @NewRole",
                    new SqlParameter("@UserId", dto.UserId),
                    new SqlParameter("@NewRole", dto.NewRole)
                );

                _logger.LogInformation("Rol değişikliği SP ile tamamlandı: {UserId} -> {NewRole}", dto.UserId, dto.NewRole);

                return ResponseGeneric<bool>.Success(true, "Kullanıcı rolü başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                return ResponseGeneric<bool>.Error("Rol güncellenirken hata oluştu: " + ex.Message);
            }
        }






    }
}

