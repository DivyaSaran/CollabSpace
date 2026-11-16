using System.Collections.Generic;
using System.Threading.Tasks;
using CollabSpace.DTOs;

namespace CollabSpace.Repositories
{
    public interface IUserRepository
    {
        /// <summary>
        /// Returns a paginated list of users along with the total count.
        /// </summary>
        Task<(IEnumerable<UserDto> Users, int TotalCount)> GetPagedUsersAsync(int pageNumber, int pageSize);

        /// <summary>
        /// Finds a user by their unique string ID.
        /// </summary>
        Task<UserDto?> GetUserByIdAsync(string id);

        /// <summary>
        /// Creates a new user with Identity validation and assigns an initial role.
        /// </summary>
        Task<IdentityResultStatus> CreateUserAsync(UserCreateUpdateDto dto);

        /// <summary>
        /// Updates user profile fields and updates password if provided.
        /// </summary>
        Task<IdentityResultStatus> UpdateUserAsync(UserCreateUpdateDto dto);

        /// <summary>
        /// Deletes a user by their ID.
        /// </summary>
        Task<IdentityResultStatus> DeleteUserAsync(string id);
    }

    /// <summary>
    /// A lightweight result record conveying whether Identity operations succeeded or returned errors.
    /// </summary>
    public record IdentityResultStatus(bool Succeeded, IEnumerable<string> Errors);
}