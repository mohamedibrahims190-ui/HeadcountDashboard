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
        string updatedBy,
        string currentShift)
        {
            if (model.Departments == null || !model.Departments.Any())
            {
                throw new ArgumentException(
                    "No department headcount data was provided.");
            }

            var duplicateDepartmentIds = model.Departments
            .GroupBy(d => d.DepartmentId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

            if (duplicateDepartmentIds.Any())
            {
                throw new ArgumentException(
                    "Duplicate department headcount data was provided.");
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

                if (department.AShiftCount > 1000 ||
                    department.BShiftCount > 1000 ||
                    department.CShiftCount > 1000)
                {
                    throw new ArgumentException(
                        "Headcount values cannot exceed 1000.");
                }
                if (existingHeadcounts.TryGetValue(
                department.DepartmentId,
                out var existingHeadcount))
                {
                    bool shiftChanged = false;

                    switch (currentShift)
                    {
                        case "A":
                            if (existingHeadcount.AShiftCount != department.AShiftCount)
                            {
                                existingHeadcount.AShiftCount = department.AShiftCount;
                                shiftChanged = true;
                            }
                            break;

                        case "B":
                            if (existingHeadcount.BShiftCount != department.BShiftCount)
                            {
                                existingHeadcount.BShiftCount = department.BShiftCount;
                                shiftChanged = true;
                            }
                            break;

                        case "C":
                            if (existingHeadcount.CShiftCount != department.CShiftCount)
                            {
                                existingHeadcount.CShiftCount = department.CShiftCount;
                                shiftChanged = true;
                            }
                            break;

                        default:
                            throw new ArgumentException(
                                "Invalid current shift.");
                    }

                    // Update audit information only when
                    // the active shift count actually changed.
                    if (shiftChanged)
                    {
                        existingHeadcount.UpdatedAt = DateTime.UtcNow;
                        existingHeadcount.UpdatedBy = updatedBy;
                    }
                }
                else
                {
                    // Create new record
                    var newHeadcount = new DailyHeadcount
                    {
                        DepartmentId = department.DepartmentId,
                        BusinessDate = businessDate,

                        AShiftCount = currentShift == "A"
        ? department.AShiftCount
        : 0,

                        BShiftCount = currentShift == "B"
        ? department.BShiftCount
        : 0,

                        CShiftCount = currentShift == "C"
        ? department.CShiftCount
        : 0,

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