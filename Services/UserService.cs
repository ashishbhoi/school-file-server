using Microsoft.EntityFrameworkCore;
using SchoolFileServer.Data;
using SchoolFileServer.Models;
using System.Text.Json;

namespace SchoolFileServer.Services
{
    public class UserService : IUserService
    {
        private readonly SchoolFileContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(SchoolFileContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UserAccount?> AuthenticateAsync(string username, string password)
        {
            var user = await GetUserByUsernameAsync(username);
            
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("Authentication failed for user: {Username}", username);
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                _logger.LogWarning("Invalid password for user: {Username}", username);
                return null;
            }

            _logger.LogInformation("User authenticated successfully: {Username}", username);
            return user;
        }

        public async Task<UserAccount?> GetUserAsync(int userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<UserAccount?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<List<UserAccount>> GetTeachersAsync()
        {
            return await _context.Users
                .Where(u => u.UserType == UserType.Teacher && u.IsActive)
                .OrderBy(u => u.Username)
                .ToListAsync();
        }

        public async Task<UserAccount> CreateUserAsync(string username, string password, UserType userType, List<string> assignedClasses)
        {
            var user = new UserAccount
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                UserType = userType,
                AssignedClasses = JsonSerializer.Serialize(assignedClasses),
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User created successfully: {Username}", username);
            return user;
        }

        public async Task<bool> UpdateUserAsync(UserAccount user)
        {
            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("User updated successfully: {UserId}", user.UserId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", user.UserId);
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await GetUserAsync(userId);
            if (user == null)
            {
                return false;
            }

            try
            {
                // Soft delete by setting IsActive to false
                user.IsActive = false;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("User deleted successfully: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }
    }
}
