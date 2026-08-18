using System.ComponentModel.DataAnnotations;

namespace HeadcountDashboard.Models
{
    public class DailyHeadcount
    {
        public int Id { get; set; }

        public int DepartmentId { get; set; }

        public Department Department { get; set; } = null!;

        public DateTime BusinessDate { get; set; }

        public int AShiftCount { get; set; }

        public int BShiftCount { get; set; }

        public int CShiftCount { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string? UpdatedBy { get; set; }
    }
}