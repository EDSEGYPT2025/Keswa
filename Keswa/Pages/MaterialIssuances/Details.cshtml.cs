using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Keswa.Pages.MaterialIssuances
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public MaterialIssuanceNote MaterialIssuanceNote { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var materialissuancenote = await _context.MaterialIssuanceNotes
                .Include(m => m.Warehouse)
                .Include(m => m.WorkOrder)
                .Include(m => m.Details)!
                .ThenInclude(d => d.Material)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (materialissuancenote == null)
            {
                return NotFound();
            }

            MaterialIssuanceNote = materialissuancenote;

            return Page();
        }
    }
}
