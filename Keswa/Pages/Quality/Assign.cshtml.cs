using System.Linq;
using System.Threading.Tasks;
using Keswa.Data;
using Keswa.Models;
using Keswa.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Keswa.Pages.Quality
{
    public class AssignModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IQualityService _qualityService;

        public AssignModel(ApplicationDbContext context, IQualityService qualityService)
        {
            _context = context;
            _qualityService = qualityService;
        }

        [BindProperty]
        public QualityInspection Inspection { get; set; }

        [BindProperty]
        public string SelectedWorkerId { get; set; }

        public SelectList Workers { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Inspection = await _context.QualityInspections
                .Include(q => q.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Inspection == null)
            {
                return NotFound();
            }

            Workers = new SelectList(_context.Users, "Id", "UserName");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _qualityService.AssignTaskToWorkerAsync(Inspection.Id, SelectedWorkerId);

            return RedirectToPage("./Index");
        }
    }
}
