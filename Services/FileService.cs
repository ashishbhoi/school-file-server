using Microsoft.EntityFrameworkCore;
using SchoolFileServer.Data;
using SchoolFileServer.Models;
using SchoolFileServer.Utilities;

namespace SchoolFileServer.Services
{
    public class FileService : IFileService
    {
        private readonly SchoolFileContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<FileService> _logger;

        // Maximum file size: 100MB
        private const long MaxFileSize = 100 * 1024 * 1024;

        // Allowed file extensions
        private readonly string[] _allowedExtensions = {
            ".pdf", ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp",
            ".mp4", ".avi", ".mov", ".wmv", ".flv", ".webm",
            ".mp3", ".wav", ".flac", ".aac", ".ogg",
            ".txt", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx"
        };

        // Dangerous file extensions to block
        private readonly string[] _dangerousExtensions = {
            ".exe", ".bat", ".cmd", ".com", ".pif", ".scr", ".vbs", ".js", 
            ".jar", ".asp", ".aspx", ".php", ".ps1", ".sh"
        };

        public FileService(SchoolFileContext context, IWebHostEnvironment webHostEnvironment, ILogger<FileService> logger)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public bool ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("File is null or empty");
                return false;
            }

            if (file.Length > MaxFileSize)
            {
                _logger.LogWarning("File size exceeds maximum allowed size: {FileSize}", file.Length);
                return false;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (_dangerousExtensions.Contains(extension))
            {
                _logger.LogWarning("Dangerous file extension detected: {Extension}", extension);
                return false;
            }

            if (!_allowedExtensions.Contains(extension))
            {
                _logger.LogWarning("File extension not allowed: {Extension}", extension);
                return false;
            }

            return true;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string className, string subject, int userId)
        {
            var classPath = DirectoryHelper.GetClassPath(className);
            var subjectPath = DirectoryHelper.GetSubjectPath(className, subject);
            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, subjectPath);

            await DirectoryHelper.EnsureDirectoryExistsAsync(fullPath);

            var originalExtension = Path.GetExtension(file.FileName);
            var safeFileName = FileHelper.GenerateSafeFileName(file.FileName);
            var uniqueFileName = $"{Path.GetFileNameWithoutExtension(safeFileName)}_{Guid.NewGuid():N}{originalExtension}";
            
            var filePath = Path.Combine(fullPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("File saved successfully: {FilePath} by user {UserId}", filePath, userId);
            return Path.Combine(subjectPath, uniqueFileName).Replace("\\", "/");
        }

        public async Task<SchoolFile?> GetFileAsync(int fileId)
        {
            return await _context.Files
                .Include(f => f.UploadedByUser)
                .FirstOrDefaultAsync(f => f.FileId == fileId);
        }

        public async Task<List<SchoolFile>> GetFilesByClassAsync(string? className = null, string? subject = null)
        {
            var query = _context.Files.Include(f => f.UploadedByUser).AsQueryable();

            if (!string.IsNullOrEmpty(className))
            {
                query = query.Where(f => f.Class == className);
            }

            if (!string.IsNullOrEmpty(subject))
            {
                query = query.Where(f => f.Subject == subject);
            }

            return await query
                .OrderBy(f => f.Class)
                .ThenBy(f => f.Subject)
                .ThenBy(f => f.FileName)
                .ToListAsync();
        }

        public async Task<List<SchoolFile>> SearchFilesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetFilesByClassAsync();
            }

            return await _context.Files
                .Include(f => f.UploadedByUser)
                .Where(f => f.FileName.Contains(searchTerm) || 
                           f.Subject.Contains(searchTerm) || 
                           (f.Description != null && f.Description.Contains(searchTerm)))
                .OrderBy(f => f.Class)
                .ThenBy(f => f.Subject)
                .ThenBy(f => f.FileName)
                .ToListAsync();
        }

        public async Task<bool> DeleteFileAsync(int fileId, int userId)
        {
            var file = await GetFileAsync(fileId);
            if (file == null)
            {
                return false;
            }

            try
            {
                // Delete physical file
                var physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, file.FilePath.TrimStart('/'));
                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                }

                // Delete database record
                _context.Files.Remove(file);
                await _context.SaveChangesAsync();

                _logger.LogInformation("File deleted successfully: {FileId} by user {UserId}", fileId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FileId}", fileId);
                return false;
            }
        }

        public async Task<SchoolFile> SaveFileRecordAsync(SchoolFile fileRecord)
        {
            _context.Files.Add(fileRecord);
            await _context.SaveChangesAsync();
            return fileRecord;
        }

        public string GetContentType(string fileExtension)
        {
            return fileExtension.ToLowerInvariant() switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                ".mp4" => "video/mp4",
                ".avi" => "video/x-msvideo",
                ".mov" => "video/quicktime",
                ".wmv" => "video/x-ms-wmv",
                ".flv" => "video/x-flv",
                ".webm" => "video/webm",
                ".mp3" => "audio/mpeg",
                ".wav" => "audio/wav",
                ".flac" => "audio/flac",
                ".aac" => "audio/aac",
                ".ogg" => "audio/ogg",
                ".txt" => "text/plain",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/octet-stream"
            };
        }

        public async Task<(string? previousId, string? nextId)> GetAdjacentFilesAsync(int currentFileId, string className, string subject)
        {
            var files = await _context.Files
                .Where(f => f.Class == className && f.Subject == subject)
                .OrderBy(f => f.FileName)
                .Select(f => f.FileId)
                .ToListAsync();

            var currentIndex = files.IndexOf(currentFileId);
            if (currentIndex == -1)
            {
                return (null, null);
            }

            var previousId = currentIndex > 0 ? files[currentIndex - 1].ToString() : null;
            var nextId = currentIndex < files.Count - 1 ? files[currentIndex + 1].ToString() : null;

            return (previousId, nextId);
        }
    }
}
