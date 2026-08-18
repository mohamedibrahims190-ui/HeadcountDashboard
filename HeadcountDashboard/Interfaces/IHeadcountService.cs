using HeadcountDashboard.ViewModels;

namespace HeadcountDashboard.Interfaces
{
    public interface IHeadcountService
    {
        Task<DashboardViewModel> GetDashboardAsync(DateTime businessDate);

        Task SaveHeadcountsAsync(
        DateTime businessDate,
        DashboardViewModel model,
        string updatedBy);
    }
}