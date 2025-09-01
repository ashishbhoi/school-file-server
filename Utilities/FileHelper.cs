using System.Text.RegularExpressions;

namespace SchoolFileServer.Utilities
{
    public static class FileHelper
    {
        public static string GenerateSafeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var cleanFileName = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
            
            // Remove multiple underscores and trim
            cleanFileName = Regex.Replace(cleanFileName, "_+", "_").Trim('_');
            
            // Ensure filename is not too long
            if (cleanFileName.Length > 100)
            {
                var extension = Path.GetExtension(cleanFileName);
                var nameWithoutExt = Path.GetFileNameWithoutExtension(cleanFileName);
                cleanFileName = nameWithoutExt.Substring(0, 100 - extension.Length) + extension;
            }

            return cleanFileName;
        }

        public static string FormatFileSize(long sizeInBytes)
        {
            if (sizeInBytes < 1024)
                return $"{sizeInBytes} B";
            
            if (sizeInBytes < 1024 * 1024)
                return $"{sizeInBytes / 1024.0:F1} KB";
                
            if (sizeInBytes < 1024 * 1024 * 1024)
                return $"{sizeInBytes / (1024.0 * 1024.0):F1} MB";
                
            return $"{sizeInBytes / (1024.0 * 1024.0 * 1024.0):F1} GB";
        }

        public static bool IsImageFile(string fileExtension)
        {
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            return imageExtensions.Contains(fileExtension.ToLowerInvariant());
        }

        public static bool IsVideoFile(string fileExtension)
        {
            var videoExtensions = new[] { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".webm" };
            return videoExtensions.Contains(fileExtension.ToLowerInvariant());
        }

        public static bool IsAudioFile(string fileExtension)
        {
            var audioExtensions = new[] { ".mp3", ".wav", ".flac", ".aac", ".ogg" };
            return audioExtensions.Contains(fileExtension.ToLowerInvariant());
        }

        public static bool IsPdfFile(string fileExtension)
        {
            return fileExtension.ToLowerInvariant() == ".pdf";
        }

        public static bool IsTextFile(string fileExtension)
        {
            return fileExtension.ToLowerInvariant() == ".txt";
        }

        public static bool IsWordFile(string fileExtension)
        {
            var wordExtensions = new[] { ".doc", ".docx" };
            return wordExtensions.Contains(fileExtension.ToLowerInvariant());
        }

        public static bool IsPowerPointFile(string fileExtension)
        {
            var pptExtensions = new[] { ".ppt", ".pptx" };
            return pptExtensions.Contains(fileExtension.ToLowerInvariant());
        }

        public static bool IsExcelFile(string fileExtension)
        {
            var excelExtensions = new[] { ".xls", ".xlsx" };
            return excelExtensions.Contains(fileExtension.ToLowerInvariant());
        }

        public static bool IsOfficeFile(string fileExtension)
        {
            return IsWordFile(fileExtension) || IsPowerPointFile(fileExtension) || IsExcelFile(fileExtension);
        }

        public static string GetFileIcon(string fileExtension)
        {
            return fileExtension.ToLowerInvariant() switch
            {
                ".pdf" => "📄",
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" => "🖼️",
                ".mp4" or ".avi" or ".mov" or ".wmv" or ".flv" or ".webm" => "🎥",
                ".mp3" or ".wav" or ".flac" or ".aac" or ".ogg" => "🎵",
                ".txt" => "📝",
                ".doc" or ".docx" => "📘",
                ".ppt" or ".pptx" => "📊",
                ".xls" or ".xlsx" => "📗",
                _ => "📎"
            };
        }

        public static string GetFormattedFileSize(long bytes)
        {
            const long kb = 1024;
            const long mb = kb * 1024;
            const long gb = mb * 1024;

            if (bytes >= gb)
                return $"{bytes / (double)gb:F2} GB";
            else if (bytes >= mb)
                return $"{bytes / (double)mb:F2} MB";
            else if (bytes >= kb)
                return $"{bytes / (double)kb:F2} KB";
            else
                return $"{bytes} bytes";
        }
    }
}
