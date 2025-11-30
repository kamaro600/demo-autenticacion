using Domain.Entities;

namespace Application.Interfaces;

public interface IUserService
{
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> ExistsAsync(string email);
    Task<User> CreateAsync(User user, string password);
    Task<bool> ValidatePasswordAsync(User user, string password);
    Task<bool> UpdateAsync(User user);
    Task<bool> DeleteAsync(string id);
    Task<ExternalLogin?> GetExternalLoginAsync(string provider, string providerUserId);
    Task<bool> AddExternalLoginAsync(User user, ExternalLogin externalLogin);
}