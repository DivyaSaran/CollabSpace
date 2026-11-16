namespace CollabSpace.DTOs
{
    public class GitItemDto
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Type { get; set; } = "file"; // "file" or "dir"
        public long Size { get; set; }
        public string? DownloadUrl { get; set; }
        public string? HtmlUrl { get; set; }
    }

    public class WorkspaceRepoConfigDto
    {
        public string Owner { get; set; } = string.Empty;
        public string Repo { get; set; } = string.Empty;
        public string Branch { get; set; } = "main";
    }
}