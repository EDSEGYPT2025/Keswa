using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Keswa.Pages.Departments
{
    public class CuttingCompletedDetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public CuttingCompletedDetailsModel(ApplicationDbContext context) => _context = context;

        [BindProperty(SupportsGet = true)]
        public int WorkOrderId { get; set; }

        public WorkOrder? WorkOrder { get; set; }
        public Customer? Customer { get; set; }
        public List<ProductSummaryDto> ProductsSummary { get; set; } = new();
        public List<CuttingStatement> CuttingStatements { get; set; } = new();
        public MaterialReturnNote? MaterialReturnNote { get; set; }

        public async Task<IActionResult> OnGetAsync(int workOrderId)
        {
            WorkOrderId = workOrderId;

            WorkOrder = await _context.WorkOrders
                .Include(w => w.Product)
                .FirstOrDefaultAsync(w => w.Id == workOrderId);

            if (WorkOrder == null) return NotFound();

            var salesOrderDetail = await _context.SalesOrderDetails
                .Include(sod => sod.SalesOrder.Customer)
                .FirstOrDefaultAsync(sod => sod.WorkOrderId == workOrderId);

            if (salesOrderDetail != null)
            {
                Customer = salesOrderDetail.SalesOrder.Customer;
            }

            MaterialReturnNote = await _context.MaterialReturnNotes
                .Include(mrn => mrn.Details)
                    .ThenInclude(d => d.Material)
                        .ThenInclude(m => m.Color)
                .FirstOrDefaultAsync(mrn => mrn.WorkOrderId == workOrderId);

            CuttingStatements = await _context.CuttingStatements
                .Where(cs => cs.WorkOrderId == workOrderId)
                .Include(cs => cs.Material).ThenInclude(m => m.Color)
                .Include(cs => cs.Product)
                .Include(cs => cs.Worker)
                .Include(cs => cs.Customer)
                .OrderBy(cs => cs.RunNumber)
                .ToListAsync();

            ProductsSummary = CuttingStatements
                .GroupBy(cs => cs.Product.Name)
                .Select(g => new ProductSummaryDto
                {
                    ProductName = g.Key,
                    TotalQuantity = g.Sum(x => x.Count)
                })
                .ToList();

            return Page();
        }


        // Keswa/Pages/Departments/CuttingCompletedDetails.cshtml.cs

        public async Task<IActionResult> OnPostTransferToSewingAsync(int statementId)
        {
            var statement = await _context.CuttingStatements
                .Include(cs => cs.WorkOrder)
                .FirstOrDefaultAsync(cs => cs.Id == statementId);

            if (statement == null) return NotFound();

            var workOrder = statement.WorkOrder;
            if (workOrder == null)
            {
                TempData["ErrorMessage"] = "خطأ: بيان القص غير مرتبط بأمر شغل.";
                return RedirectToPage(new { workOrderId = statement.WorkOrderId });
            }

            // --- بداية الحل القاطع ---

            // 1. نبحث عن كل أرقام تشغيلات الخياطة الحالية التابعة لنفس أمر الشغل
            var existingSewingBatchNumbers = await _context.SewingBatches
                .Where(sb => sb.CuttingStatement.WorkOrderId == workOrder.Id)
                .Select(sb => sb.SewingBatchNumber)
                .ToListAsync();

            int maxSequence = 0;
            foreach (var number in existingSewingBatchNumbers)
            {
                // 2. نستخرج الرقم التسلسلي من كل رقم موجود (مثال: من "WO-2025-2-001-SEW" نستخرج "001")
                var parts = number.Split('-');
                // نتأكد أن الرقم يحتوي على 4 أجزاء على الأقل قبل محاولة استخراج الرقم
                if (parts.Length >= 4 && int.TryParse(parts[2], out int currentSequence))
                {
                    if (currentSequence > maxSequence)
                    {
                        maxSequence = currentSequence; // 3. نحدد أعلى رقم تسلسلي تم استخدامه
                    }
                }
            }

            // 4. الرقم التسلسلي الجديد هو أعلى رقم + 1
            var nextSequence = maxSequence + 1;
            var newSewingBatchNumber = $"{workOrder.WorkOrderNumber}-{nextSequence:D3}-SEW";

            // --- نهاية الحل القاطع ---

            var sewingBatch = new SewingBatch
            {
                SewingBatchNumber = newSewingBatchNumber,
                CuttingStatementId = statement.Id,
                Quantity = statement.Count,
                Status = BatchStatus.PendingTransfer,
                CreationDate = Helpers.DateTimeHelper.EgyptNow
            };
            _context.SewingBatches.Add(sewingBatch);

            statement.Status = BatchStatus.Transferred;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"تم تحويل التشغيلة بنجاح. الرقم الجديد هو: {newSewingBatchNumber}";
            return RedirectToPage(new { workOrderId = statement.WorkOrderId });
        }
    }

    public class ProductSummaryDto
    {
        public string ProductName { get; set; }
        public int TotalQuantity { get; set; }
    }

    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .First()
                            .GetCustomAttribute<DisplayAttribute>()
                            ?.GetName() ?? enumValue.ToString();
        }
    }
}