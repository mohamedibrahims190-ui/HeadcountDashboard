using HeadcountDashboard.Data;
using HeadcountDashboard.Interfaces;
using HeadcountDashboard.Models;
using HeadcountDashboard.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HeadcountDashboard.Services
{
    public class HeadcountService : IHeadcountService
    {
        private readonly ApplicationDbContext _context;

        public HeadcountService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardAsync(
            DateTime businessDate)
        {
            var departments = await _context.Departments
                .Where(d => d.IsActive)
                .OrderBy(d => d.Id)
                .ToListAsync();

            var headcounts = await _context.DailyHeadcounts
                .Where(h => h.BusinessDate == businessDate)
                .ToListAsync();

            var model = new DashboardViewModel
            {
                BusinessDate = businessDate,

                Departments = departments.Select(department =>
                {
                    var headcount = headcounts
                        .FirstOrDefault(h =>
                            h.DepartmentId == department.Id);

                    return new DepartmentHeadcountViewModel
                    {
                        DepartmentId = department.Id,
                        DepartmentName = department.Name,
                        DepartmentCode = department.Code,

                        AShiftCount = headcount?.AShiftCount ?? 0,
                        BShiftCount = headcount?.BShiftCount ?? 0,
                        CShiftCount = headcount?.CShiftCount ?? 0,

                        UpdatedBy = headcount?.UpdatedBy,
                        UpdatedAt = headcount?.UpdatedAt
                    };
                }).ToList() 
            };

            return model;
        }

        public async Task SaveHeadcountsAsync(
        DateTime businessDate,
        DashboardViewModel model,
        string updatedBy)
        {
            if (model.Departments == null || !model.Departments.Any())
            {
                return;
            }

            // Load all existing headcounts for the selected date in one query
            var existingHeadcounts = await _context.DailyHeadcounts
                .Where(h => h.BusinessDate == businessDate)
                .ToDictionaryAsync(h => h.DepartmentId);

            var activeDepartmentIds = await _context.Departments
                .Where(d => d.IsActive)
                .Select(d => d.Id)
                .ToHashSetAsync();

            foreach (var department in model.Departments)
            {
                if (!activeDepartmentIds.Contains(department.DepartmentId))
                {
                    throw new ArgumentException(
                        $"Invalid or inactive department: {department.DepartmentId}");
                }
                // Server-side validation
                if (department.AShiftCount < 0 ||
                    department.BShiftCount < 0 ||
                    department.CShiftCount < 0)
                {
                    throw new ArgumentException(
                        "Headcount values cannot be negative.");
                }

                if (existingHeadcounts.TryGetValue(
                    department.DepartmentId,
                    out var existingHeadcount))
                {
                    // Update existing record
                    existingHeadcount.AShiftCount = department.AShiftCount;
                    existingHeadcount.BShiftCount = department.BShiftCount;
                    existingHeadcount.CShiftCount = department.CShiftCount;
                    existingHeadcount.UpdatedAt = DateTime.UtcNow;
                    existingHeadcount.UpdatedBy = updatedBy;
                }
                else
                {
                    // Create new record
                    var newHeadcount = new DailyHeadcount
                    {
                        DepartmentId = department.DepartmentId,
                        BusinessDate = businessDate,

                        AShiftCount = department.AShiftCount,
                        BShiftCount = department.BShiftCount,
                        CShiftCount = department.CShiftCount,
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedBy = updatedBy
                    };

                    await _context.DailyHeadcounts.AddAsync(newHeadcount);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}