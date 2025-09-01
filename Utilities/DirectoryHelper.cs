namespace SchoolFileServer.Utilities
{
    public static class DirectoryHelper
    {
        public static string GetClassPath(string className)
        {
            return Path.Combine("uploads", $"Class {className}");
        }

        public static string GetSubjectPath(string className, string subject)
        {
            return Path.Combine(GetClassPath(className), subject);
        }

        public static async Task EnsureDirectoryExistsAsync(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            await Task.CompletedTask;
        }

        public static List<string> GetAllClassDirectories(string webRootPath)
        {
            var uploadsPath = Path.Combine(webRootPath, "uploads");
            if (!Directory.Exists(uploadsPath))
            {
                return new List<string>();
            }

            return Directory.GetDirectories(uploadsPath)
                .Select(d => Path.GetFileName(d))
                .Where(d => d.StartsWith("Class "))
                .OrderBy(d => d)
                .ToList();
        }

        public static List<string> GetSubjectDirectories(string webRootPath, string className)
        {
            var classPath = Path.Combine(webRootPath, GetClassPath(className));
            if (!Directory.Exists(classPath))
            {
                return new List<string>();
            }

            return Directory.GetDirectories(classPath)
                .Select(d => Path.GetFileName(d))
                .OrderBy(d => d)
                .ToList();
        }
    }
}
