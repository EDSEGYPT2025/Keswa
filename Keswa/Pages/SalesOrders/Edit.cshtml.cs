using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Keswa.Pages.SalesOrders
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public SalesOrder SalesOrder { get; set; } = default!;

        public SelectList CustomerList { get; set; }
        public SelectList ProductList { get; set; }

        public bool IsEditable { get; set; } = true;
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            SalesOrder = await _context.SalesOrders
                .Include(s => s.Customer)
                .Include(s => s.Details)!
                .ThenInclude(d => d.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (SalesOrder == null)
            {
                return NotFound();
            }

            // تم تعديل الشرط هنا ليشمل كل الحالات ما عدا "جديدة"
            if (SalesOrder.Status != SalesOrderStatus.New)
            {
                IsEditable = false;
                StatusMessage = "لا يمكن تعديل هذه الطلبية لأنه تم البدء في تنفيذها.";
            }

            CustomerList = new SelectList(await _context.Customers.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", SalesOrder.CustomerId);
            ProductList = new SelectList(await _context.Products.OrderBy(p => p.Name).ToListAsync(), "Id", "Name");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var originalOrder = await _context.SalesOrders
                .Include(s => s.Details)
                .FirstOrDefaultAsync(s => s.Id == SalesOrder.Id);

            if (originalOrder == null)
            {
                return NotFound();
            }

            // التحقق مرة أخرى من الحالة قبل الحفظ
            if (originalOrder.Status != SalesOrderStatus.New)
            {
                ModelState.AddModelError(string.Empty, "لا يمكن تعديل هذه الطلبية لأنه تم تغيير حالتها.");
                await OnGetAsync(SalesOrder.Id);
                return Page();
            }

            // تحديث البيانات الأساسية
            _context.Entry(originalOrder).CurrentValues.SetValues(SalesOrder);

            var submittedDetailIds = SalesOrder.Details?.Select(d => d.Id).ToHashSet() ?? new HashSet<int>();

            // *** تم التعديل هنا: حذف البنود التي لم يتم تحويلها فقط ***
            var detailsToDelete = originalOrder.Details
                .Where(d => !submittedDetailIds.Contains(d.Id) && d.Status == SalesOrderDetailStatus.PendingConversion)
                .ToList();
            _context.SalesOrderDetails.RemoveRange(detailsToDelete);

            if (SalesOrder.Details != null)
            {
                foreach (var detail in SalesOrder.Details)
                {
                    if (detail.Id > 0)
                    {
                        var existingDetail = originalOrder.Details.FirstOrDefault(d => d.Id == detail.Id);
                        // *** تم التعديل هنا: تعديل البنود التي لم يتم تحويلها فقط ***
                        if (existingDetail != null && existingDetail.Status == SalesOrderDetailStatus.PendingConversion)
                        {
                            _context.Entry(existingDetail).CurrentValues.SetValues(detail);
                        }
                    }
                    else
                    {
                        originalOrder.Details.Add(detail);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}
