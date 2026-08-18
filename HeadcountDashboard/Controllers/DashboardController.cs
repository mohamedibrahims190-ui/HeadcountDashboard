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

        public DashboardController(IHeadcountService headcountService)
        {
            _headcountService = headcountService;
        }

        public async Task<IActionResult> Index(DateTime? date)
        {
            var businessDate = date?.Date ?? DateTime.Today;

            var model = await _headcountService
                .GetDashboardAsync(businessDate);

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
                await _headcountService.SaveHeadcountsAsync(
                    businessDate,
                    model,
                    updatedBy);

                TempData["SuccessMessage"] =
                    "Headcount saved successfully.";
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(
                nameof(Index),
                new { date = businessDate });
        }
    }
}