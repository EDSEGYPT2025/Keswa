using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.SalesOrders
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public SalesOrder SalesOrder { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var salesorder = await _context.SalesOrders
                .Include(s => s.Customer)
                .Include(s => s.Details)!
                .ThenInclude(d => d.Product)
                .Include(s => s.Details)!
                .ThenInclude(d => d.WorkOrder)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (salesorder == null)
            {
                return NotFound();
            }
            else
            {
                SalesOrder = salesorder;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostConvertToWorkOrderAsync(int salesOrderDetailId)
        {
            var salesOrderDetail = await _context.SalesOrderDetails
                .Include(d => d.SalesOrder)
                .FirstOrDefaultAsync(d => d.Id == salesOrderDetailId);

            if (salesOrderDetail == null || salesOrderDetail.Status == SalesOrderDetailStatus.ConvertedToWorkOrder)
            {
                return RedirectToPage(new { id = salesOrderDetail?.SalesOrderId });
            }

            // 1. Create a new Work Order
            var newWorkOrder = new WorkOrder
            {
                ProductId = salesOrderDetail.ProductId,
                QuantityToProduce = salesOrderDetail.Quantity,
                Status = WorkOrderStatus.New,
                CreationDate = DateTime.Today,
                PlannedStartDate = DateTime.Today,
                PlannedCompletionDate = salesOrderDetail.SalesOrder.ExpectedDeliveryDate.AddDays(-2)
            };

            _context.WorkOrders.Add(newWorkOrder);
            await _context.SaveChangesAsync(); // First save to generate the ID

            // 2. Generate and assign the WorkOrderNumber
            var countForYear = await _context.WorkOrders.CountAsync(wo => wo.CreationDate.Year == newWorkOrder.CreationDate.Year);
            newWorkOrder.WorkOrderNumber = $"WO-{newWorkOrder.CreationDate.Year}-{countForYear}";

            // *** تم التعديل هنا: إنشاء مسار الإنتاج تلقائياً ***
            // 3. Create the production routing stages for the new work order
            var departments = Enum.GetValues(typeof(Department)).Cast<Department>();
            foreach (var dept in departments)
            {
                _context.WorkOrderRoutings.Add(new WorkOrderRouting
                {
                    WorkOrderId = newWorkOrder.Id,
                    Department = dept,
                    Status = WorkOrderStageStatus.Pending
                });
            }

            // 4. Update the Sales Order Detail
            salesOrderDetail.Status = SalesOrderDetailStatus.ConvertedToWorkOrder;
            salesOrderDetail.WorkOrderId = newWorkOrder.Id;

            // 5. Update the main Sales Order status
            var orderDetails = await _context.SalesOrderDetails
                .Where(d => d.SalesOrderId == salesOrderDetail.SalesOrderId)
                .ToListAsync();

            if (orderDetails.All(d => d.Status == SalesOrderDetailStatus.ConvertedToWorkOrder))
            {
                salesOrderDetail.SalesOrder.Status = SalesOrderStatus.Completed;
            }
            else
            {
                salesOrderDetail.SalesOrder.Status = SalesOrderStatus.InProgress;
            }

            await _context.SaveChangesAsync(); // Save all subsequent changes

            TempData["SuccessMessage"] = $"تم تحويل البند إلى أمر الشغل رقم {newWorkOrder.WorkOrderNumber} بنجاح.";
            return RedirectToPage(new { id = salesOrderDetail.SalesOrderId });
        }
    }
}

