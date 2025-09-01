using SchoolFileServer.Models;

namespace SchoolFileServer.Services
{
    public interface IFileService
    {
        bool ValidateFile(IFormFile file);
        Task<string> SaveFileAsync(IFormFile file, string className, string subject, int userId);
        Task<SchoolFile?> GetFileAsync(int fileId);
        Task<List<SchoolFile>> GetFilesByClassAsync(string? className = null, string? subject = null);
        Task<List<SchoolFile>> SearchFilesAsync(string searchTerm);
        Task<bool> DeleteFileAsync(int fileId, int userId);
        Task<SchoolFile> SaveFileRecordAsync(SchoolFile fileRecord);
        string GetContentType(string fileExtension);
        Task<(string? previousId, string? nextId)> GetAdjacentFilesAsync(int currentFileId, string className, string subject);
    }
}
