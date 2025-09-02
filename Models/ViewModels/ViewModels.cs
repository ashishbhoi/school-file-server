using System.ComponentModel.DataAnnotations;

namespace SchoolFileServer.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }

    public class FileUploadViewModel
    {
        [Required]
        public IFormFile File { get; set; } = null!;

        [Required]
        [StringLength(10)]
        public string Class { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Subject { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
    }

    public class FileBrowserViewModel
    {
        public string? SelectedClass { get; set; }
        public string? SelectedSubject { get; set; }
        public List<SchoolClass> Classes { get; set; } = new();
        public List<Subject> Subjects { get; set; } = new();
        public List<SchoolFile> Files { get; set; } = new();
        public string SearchTerm { get; set; } = string.Empty;
    }

    public class FileViewerViewModel
    {
        public SchoolFile File { get; set; } = null!;
        public string ContentType { get; set; } = string.Empty;
        public bool CanDelete { get; set; }
        public string? PreviousFileId { get; set; }
        public string? NextFileId { get; set; }
    }

    public class AdminDashboardViewModel
    {
        public List<UserAccount> Teachers { get; set; } = new();
        public List<SchoolClass> Classes { get; set; } = new();
        public int TotalFiles { get; set; }
        public long TotalFileSize { get; set; }
        public List<SchoolFile> RecentFiles { get; set; } = new();
    }

    public class CreateUserViewModel
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public UserType UserType { get; set; }

        public List<string> AssignedClasses { get; set; } = new();
        public List<SchoolClass> AvailableClasses { get; set; } = new();
    }

    public class CreateClassViewModel
    {
        [Required]
        [StringLength(10, ErrorMessage = "Class name cannot exceed 10 characters")]
        [Display(Name = "Class Name")]
        public string ClassName { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "Display name cannot exceed 100 characters")]
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; } = string.Empty;

        [Required]
        [Range(1, 1000, ErrorMessage = "Sort order must be between 1 and 1000")]
        [Display(Name = "Sort Order")]
        public int SortOrder { get; set; } = 1;
    }

    public class EditClassViewModel
    {
        public int ClassId { get; set; }

        [Required]
        [StringLength(10, ErrorMessage = "Class name cannot exceed 10 characters")]
        [Display(Name = "Class Name")]
        public string ClassName { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "Display name cannot exceed 100 characters")]
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; } = string.Empty;

        [Required]
        [Range(1, 1000, ErrorMessage = "Sort order must be between 1 and 1000")]
        [Display(Name = "Sort Order")]
        public int SortOrder { get; set; } = 1;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Read-only properties for display
        public int FileCount { get; set; }
        public string? OriginalClassName { get; set; }
    }
}
