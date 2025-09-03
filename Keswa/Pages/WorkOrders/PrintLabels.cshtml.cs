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
    public class PrintLabelsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PrintLabelsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public WorkOrder WorkOrder { get; set; }
        public List<LabelViewModel> Labels { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            WorkOrder = await _context.WorkOrders
                .Include(wo => wo.Product)
                .FirstOrDefaultAsync(wo => wo.Id == id);

            if (WorkOrder == null)
            {
                return NotFound();
            }

            var receivedProducts = await _context.ProductionReceiptLogs
                .Where(p => p.WorkOrderId == id)
                .ToListAsync();

            int serialCounter = 1;
            foreach (var receipt in receivedProducts)
            {
                for (int i = 0; i < receipt.Quantity; i++)
                {
                    var serialNumber = $"{WorkOrder.WorkOrderNumber}-{serialCounter:D4}";

                    var label = new LabelViewModel
                    {
                        FactoryName = "مصنع كسوة",
                        ProductName = WorkOrder.Product.Name,
                        WorkOrderNumber = WorkOrder.WorkOrderNumber,
                        QualityGrade = receipt.QualityGrade == "First-Grade" ? "درجة أولى" : "درجة ثانية",
                        SerialNumber = serialNumber,
                        // *** تم التعديل هنا: تبسيط البيانات ***
                        QrCodeData = $"KSW|{serialNumber}|{WorkOrder.Product.Name}"
                    };

                    Labels.Add(label);
                    serialCounter++;
                }
            }

            return Page();
        }
    }

    public class LabelViewModel
    {
        public string FactoryName { get; set; }
        public string ProductName { get; set; }
        public string WorkOrderNumber { get; set; }
        public string QualityGrade { get; set; }
        public string SerialNumber { get; set; }
        public string QrCodeData { get; set; }
    }
}
