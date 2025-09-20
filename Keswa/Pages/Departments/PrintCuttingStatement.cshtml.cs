using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Keswa.Pages.Departments
{
    public class PrintCuttingStatementModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PrintCuttingStatementModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public CuttingStatement CuttingStatement { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            CuttingStatement = await _context.CuttingStatements
                .Include(cs => cs.WorkOrder).ThenInclude(wo => wo.Product)
                .Include(cs => cs.Material).ThenInclude(m => m.Color)
                .Include(cs => cs.Worker)
                .Include(cs => cs.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (CuttingStatement == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}

