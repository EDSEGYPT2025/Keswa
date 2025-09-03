using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Keswa.Pages.PurchaseOrders
{
    public class PrintModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PrintModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public PurchaseOrder PurchaseOrder { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            PurchaseOrder = await _context.PurchaseOrders
                .Include(p => p.WorkOrder)
                .Include(p => p.Details)
                    .ThenInclude(d => d.Material)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (PurchaseOrder == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
