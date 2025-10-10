using CateringManagement.Models;

namespace CateringManagement.Repositories
{
    public interface IUserRepository
    {
        Task RegisterUser(string fullName, string email, string phone, string password, string role);
        Task<User> LoginUser(string email, string password);
        Task<IEnumerable<User>> GetAllUsers();
        Task<User> GetUserById(int userId);
        Task UpdateUserProfile(int userId, string? fullName, string? email, string? phone);
        Task ChangePassword(int userId, string oldPassword, string newPassword);
        Task DeleteUser(int userId);
    }
}
