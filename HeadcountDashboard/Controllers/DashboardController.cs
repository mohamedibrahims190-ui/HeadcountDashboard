using HeadcountDashboard.Interfaces;
using HeadcountDashboard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeadcountDashboard.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IHeadcountService _headcountService;

        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IHeadcountService headcountService,
            ILogger<DashboardController> logger)
        {
            _headcountService = headcountService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(DateTime? date)
        {
            var businessDate = date?.Date ?? DateTime.Today;

            var model = await _headcountService
                .GetDashboardAsync(businessDate);

            model.CurrentShift = GetCurrentShift();
            var currentBusinessDate = GetCurrentBusinessDate();

            model.IsEditableDate =
                model.BusinessDate.Date == currentBusinessDate.Date;

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(
            DateTime businessDate,
            DashboardViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            var updatedBy = User.Identity?.Name ?? "Unknown";

            try
            {
                var currentShift = GetCurrentShift();
                var currentBusinessDate = GetCurrentBusinessDate();

                if (businessDate.Date != currentBusinessDate.Date)
                {
                    TempData["ErrorMessage"] =
                        "Only the current business date can be edited.";

                    return RedirectToAction(
                        nameof(Index),
                        new { date = businessDate });
                }

                await _headcountService.SaveHeadcountsAsync(
                    businessDate,
                    model,
                    updatedBy,
                    currentShift);

                TempData["SuccessMessage"] =
                    "Headcount saved successfully.";
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while saving headcount for business date {BusinessDate}",
                    businessDate);

                TempData["ErrorMessage"] =
                    "An unexpected error occurred while saving the headcount. Please try again.";
            }

            return RedirectToAction(
                nameof(Index),
                new { date = businessDate });
        }
        private string GetCurrentShift()
        {
            var now = DateTime.Now.TimeOfDay;

            if (now >= TimeSpan.FromHours(6) &&
                now < TimeSpan.FromHours(14))
            {
                return "A";
            }

            if (now >= TimeSpan.FromHours(14) &&
                now < TimeSpan.FromHours(22))
            {
                return "B";
            }

            return "C";
        }

        private DateTime GetCurrentBusinessDate()
        {
            var now = DateTime.Now;

            // Business day starts at 06:00
            if (now.TimeOfDay < TimeSpan.FromHours(6))
            {
                return now.Date.AddDays(-1);
            }

            return now.Date;
        }
    }
}