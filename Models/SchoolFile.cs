using System.ComponentModel.DataAnnotations;

namespace SchoolFileServer.Models
{
    public class SchoolFile
    {
        public int FileId { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string FileType { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Class { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Subject { get; set; } = string.Empty;

        public int UploadedBy { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.Now;

        public long FileSize { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        // Navigation properties
        public virtual UserAccount? UploadedByUser { get; set; }
    }
}
