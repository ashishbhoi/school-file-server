using System.ComponentModel.DataAnnotations;

namespace SchoolFileServer.Models
{
    public class Subject
    {
        public int SubjectId { get; set; }

        [Required]
        [StringLength(100)]
        public string SubjectName { get; set; } = string.Empty;

        public int ClassId { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual SchoolClass? Class { get; set; }
        public virtual UserAccount? CreatedByUser { get; set; }
    }
}
