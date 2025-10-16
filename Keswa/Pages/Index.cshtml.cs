using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Keswa.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        // --- تمت إضافة هذه الخاصية ---
        public CuttingDeptSummaryViewModel CuttingSummary { get; set; }

        public IndexModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public string UserFullName { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                UserFullName = user.FullName;
            }

            // سنترك هذه الدالة فارغة لأن البيانات ستأتي مباشرة من SignalR
        }
    }

    // --- تمت إضافة هذا النموذج المساعد ---
    public class CuttingDeptSummaryViewModel
    {
        public int WorkOrderCount { get; set; }
        public int TotalRequired { get; set; }
        public int TotalCompleted { get; set; }
        public int Remaining => TotalRequired - TotalCompleted;
        public int ProgressPercentage => (TotalRequired > 0) ? (int)System.Math.Round((double)TotalCompleted * 100 / TotalRequired) : 0;
    }
}