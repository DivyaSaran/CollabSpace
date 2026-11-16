using System.Collections.Generic;
using System.Threading.Tasks;
using CollabSpace.DTOs;

namespace CollabSpace.Services
{
    public interface IGitRepositoryService
    {
        /// <summary>
        /// Retrieves the list of files and directories at a specific path in a repository.
        /// </summary>
        Task<(bool Succeeded, IEnumerable<GitItemDto> Items, string? ErrorMessage)> GetRepositoryContentsAsync(
            string owner,
            string repo,
            string path = "",
            string branch = "main");

        Task<(bool Succeeded, string? Content, string? ErrorMessage)> GetFileContentAsync(
            string owner,
            string repo,
            string filePath,
            string branch = "main");

    }


}