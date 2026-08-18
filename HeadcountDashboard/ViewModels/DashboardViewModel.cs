using HeadcountDashboard.Models;

namespace HeadcountDashboard.ViewModels
{
    public class DashboardViewModel
    {
        public DateTime BusinessDate { get; set; }

        public List<DepartmentHeadcountViewModel> Departments { get; set; } = new();
        public int TotalHeadcount =>
            Departments.Sum(x => x.Total);

        public int AShiftTotal =>
            Departments.Sum(x => x.AShiftCount);

        public int BShiftTotal =>
            Departments.Sum(x => x.BShiftCount);

        public int CShiftTotal =>
            Departments.Sum(x => x.CShiftCount);

    }

    public class DepartmentHeadcountViewModel
    {
        public int DepartmentId { get; set; }

        public string DepartmentName { get; set; } = string.Empty;

        public string DepartmentCode { get; set; } = string.Empty;

        public int AShiftCount { get; set; }

        public int BShiftCount { get; set; }

        public int CShiftCount { get; set; }

        public int Total =>
            AShiftCount + BShiftCount + CShiftCount;

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}