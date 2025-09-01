using System.ComponentModel.DataAnnotations;

namespace SchoolFileServer.Models
{
    public class SchoolClass
    {
        public int ClassId { get; set; }

        [Required]
        [StringLength(10)]
        public string ClassName { get; set; } = string.Empty;

        [StringLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; }

        // Navigation properties
        public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    }
}
