using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Keswa.Pages.Departments
{
    public class CuttingCompletedDetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public CuttingCompletedDetailsModel(ApplicationDbContext context) => _context = context;

        [BindProperty(SupportsGet = true)]
        public int WorkOrderId { get; set; }

        public WorkOrder? WorkOrder { get; set; }

        public List<ProductSummaryDto> ProductsSummary { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int workOrderId)
        {
            WorkOrderId = workOrderId;

            WorkOrder = await _context.WorkOrders
                .Include(w => w.Product)
                .FirstOrDefaultAsync(w => w.Id == workOrderId);

            if (WorkOrder == null) return NotFound();

            // نجمع الكميات بالمنتج
            ProductsSummary = await _context.CuttingStatements
                .Include(cs => cs.Product)
                .Where(cs => cs.WorkOrderId == workOrderId)
                .GroupBy(cs => cs.Product.Name)
                .Select(g => new ProductSummaryDto
                {
                    ProductName = g.Key,
                    TotalQuantity = g.Sum(x => x.Count)
                })
                .ToListAsync();

            return Page();
        }
    }

    public class ProductSummaryDto
    {
        public string ProductName { get; set; }
        public int TotalQuantity { get; set; }
    }
}
