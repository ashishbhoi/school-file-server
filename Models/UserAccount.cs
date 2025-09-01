using System.ComponentModel.DataAnnotations;

namespace SchoolFileServer.Models
{
    public class UserAccount
    {
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public UserType UserType { get; set; }

        /// <summary>
        /// JSON array of assigned class names (e.g., ["VI", "VII", "VIII"])
        /// </summary>
        public string AssignedClasses { get; set; } = "[]";

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<SchoolFile> UploadedFiles { get; set; } = new List<SchoolFile>();
        public virtual ICollection<Subject> CreatedSubjects { get; set; } = new List<Subject>();
    }

    public enum UserType
    {
        Admin = 1,
        Teacher = 2
    }
}
