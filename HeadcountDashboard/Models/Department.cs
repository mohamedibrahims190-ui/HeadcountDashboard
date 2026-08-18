using System.ComponentModel.DataAnnotations;

namespace HeadcountDashboard.Models
{
    public class Department
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public ICollection<DailyHeadcount> DailyHeadcounts { get; set; }
            = new List<DailyHeadcount>();
    }
}