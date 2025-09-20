using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.Departments
{
    public class PrintCuttingStatementAllModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PrintCuttingStatementAllModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public WorkOrder WorkOrder { get; set; }
        public List<CuttingStatement> CuttingStatements { get; set; }

        public async Task<IActionResult> OnGetAsync(int? workOrderId)
        {
            if (workOrderId == null)
            {
                return NotFound();
            }

            WorkOrder = await _context.WorkOrders
                .Include(wo => wo.Product)
                .FirstOrDefaultAsync(wo => wo.Id == workOrderId);

            if (WorkOrder == null)
            {
                return NotFound();
            }

            CuttingStatements = await _context.CuttingStatements
                .Where(cs => cs.WorkOrderId == workOrderId)
                .Include(cs => cs.Material).ThenInclude(m => m.Color)
                .Include(cs => cs.Worker)
                .Include(cs => cs.Customer)
                .OrderBy(cs => cs.RunNumber)
                .ToListAsync();

            return Page();
        }
    }
}
