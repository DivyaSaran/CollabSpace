using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using CollabSpace.DTOs;
using Microsoft.Extensions.Configuration;

namespace CollabSpace.Services
{
    public class GitRepositoryService : IGitRepositoryService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public GitRepositoryService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;

            // GitHub API requires a User-Agent header
            if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "CollabSpace-App");
            }

            // Optional Personal Access Token from configuration
            var token = _config["GitHub:Token"];
            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<(bool Succeeded, IEnumerable<GitItemDto> Items, string? ErrorMessage)> GetRepositoryContentsAsync(
            string owner,
            string repo,
            string path = "",
            string branch = "main")
        {
            try
            {
                var cleanPath = path.TrimStart('/');
                var url = $"https://api.github.com/repos/{owner}/{repo}/contents/{cleanPath}?ref={branch}";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    var statusCode = (int)response.StatusCode;
                    return (false, new List<GitItemDto>(), $"GitHub API error ({statusCode}): {response.ReasonPhrase}");
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                var items = new List<GitItemDto>();

                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var element in doc.RootElement.EnumerateArray())
                    {
                        items.Add(new GitItemDto
                        {
                            Name = element.GetProperty("name").GetString() ?? string.Empty,
                            Path = element.GetProperty("path").GetString() ?? string.Empty,
                            Type = element.GetProperty("type").GetString() ?? "file",
                            Size = element.TryGetProperty("size", out var s) ? s.GetInt64() : 0,
                            DownloadUrl = element.TryGetProperty("download_url", out var d) && d.ValueKind != JsonValueKind.Null ? d.GetString() : null,
                            HtmlUrl = element.TryGetProperty("html_url", out var h) ? h.GetString() : null
                        });
                    }
                }
                else if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    // Single file requested
                    items.Add(new GitItemDto
                    {
                        Name = doc.RootElement.GetProperty("name").GetString() ?? string.Empty,
                        Path = doc.RootElement.GetProperty("path").GetString() ?? string.Empty,
                        Type = doc.RootElement.GetProperty("type").GetString() ?? "file",
                        Size = doc.RootElement.TryGetProperty("size", out var s) ? s.GetInt64() : 0,
                        DownloadUrl = doc.RootElement.TryGetProperty("download_url", out var d) && d.ValueKind != JsonValueKind.Null ? d.GetString() : null,
                        HtmlUrl = doc.RootElement.TryGetProperty("html_url", out var h) ? h.GetString() : null
                    });
                }

                // Sort directories first, then alphabetically
                var sorted = items
                    .OrderByDescending(i => i.Type == "dir")
                    .ThenBy(i => i.Name)
                    .ToList();

                return (true, sorted, null);
            }
            catch (Exception ex)
            {
                return (false, new List<GitItemDto>(), $"Exception fetching repo contents: {ex.Message}");
            }
        }

        public async Task<(bool Succeeded, string? Content, string? ErrorMessage)> GetFileContentAsync(
            string owner,
            string repo,
            string filePath,
            string branch = "main")
        {
            try
            {
                var cleanPath = filePath.TrimStart('/');
                var url = $"https://api.github.com/repos/{owner}/{repo}/contents/{cleanPath}?ref={branch}";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    var statusCode = (int)response.StatusCode;
                    return (false, null, $"GitHub API error ({statusCode}): {response.ReasonPhrase}");
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                // GitHub returns base64-encoded file content for files
                if (doc.RootElement.TryGetProperty("content", out var contentElement))
                {
                    var base64Raw = contentElement.GetString() ?? string.Empty;
                    // GitHub wraps base64 strings with newlines; strip them before decoding
                    var cleanBase64 = base64Raw.Replace("\n", "").Replace("\r", "");
                    var bytes = Convert.FromBase64String(cleanBase64);
                    var decodedText = System.Text.Encoding.UTF8.GetString(bytes);
                    return (true, decodedText, null);
                }

                // Fallback: If GitHub supplied a download_url directly
                if (doc.RootElement.TryGetProperty("download_url", out var dlUrl) && dlUrl.ValueKind != JsonValueKind.Null)
                {
                    var rawResponse = await _httpClient.GetAsync(dlUrl.GetString());
                    if (rawResponse.IsSuccessStatusCode)
                    {
                        var text = await rawResponse.Content.ReadAsStringAsync();
                        return (true, text, null);
                    }
                }

                return (false, null, "Unable to extract text content from file.");
            }
            catch (Exception ex)
            {
                return (false, null, $"Error reading file content: {ex.Message}");
            }
        }
    }
}