using SchoolFileServer.Models;

namespace SchoolFileServer.Services
{
    public interface IUserService
    {
        Task<UserAccount?> AuthenticateAsync(string username, string password);
        Task<UserAccount?> GetUserAsync(int userId);
        Task<UserAccount?> GetUserByUsernameAsync(string username);
        Task<List<UserAccount>> GetTeachersAsync();
        Task<UserAccount> CreateUserAsync(string username, string password, UserType userType, List<string> assignedClasses);
        Task<bool> UpdateUserAsync(UserAccount user);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> UserExistsAsync(string username);
    }
}
