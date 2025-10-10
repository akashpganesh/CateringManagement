using CateringManagement.Models;

namespace CateringManagement.Managers
{
    public interface IUserManager
    {
        Task RegisterUser(string fullName, string email, string phone, string password);
        Task<LoginResponse> LoginUser(string email, string password);
        Task<IEnumerable<UserDto>> GetAllUsers();
        Task<UserDto> GetUserById(int userId);
        Task UpdateUserProfile(int userId, UpdateUserRequest request);
        Task ChangePassword(int userId, ChangePasswordRequest request);
        Task DeleteUser(int userId);
    }
}
