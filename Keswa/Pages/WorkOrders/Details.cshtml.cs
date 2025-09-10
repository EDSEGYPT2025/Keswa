using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.WorkOrders
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public WorkOrder WorkOrder { get; set; } = default!;

        // --- تمت إضافة كلا القائمتين ---
        public List<MaterialRequisition> MaterialRequisitions { get; set; }
        public List<MaterialIssuanceNote> MaterialIssuanceNotes { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            WorkOrder = await _context.WorkOrders
                .Include(wo => wo.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(wo => wo.Id == id);

            if (WorkOrder == null)
            {
                return NotFound();
            }

            // --- جلب طلبات الصرف ---
            MaterialRequisitions = await _context.MaterialRequisitions
                .Where(r => r.WorkOrderId == id)
                .OrderByDescending(r => r.RequestDate)
                .AsNoTracking()
                .ToListAsync();

            // --- جلب أذونات الصرف ---
            MaterialIssuanceNotes = await _context.MaterialIssuanceNotes
                .Include(i => i.Warehouse)
                .Where(i => i.WorkOrderId == id)
                .OrderByDescending(i => i.IssuanceDate)
                .AsNoTracking()
                .ToListAsync();

            return Page();
        }
    }
}

