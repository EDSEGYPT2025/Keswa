using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Keswa.Pages.SalesOrders
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public SalesOrder SalesOrder { get; set; } = default!;
        public bool IsDeletable { get; set; } = true;
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            SalesOrder = await _context.SalesOrders
                .Include(s => s.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (SalesOrder == null)
            {
                return NotFound();
            }

            // التحقق إذا كانت الطلبية قابلة للحذف
            if (SalesOrder.Status != SalesOrderStatus.New)
            {
                IsDeletable = false;
                StatusMessage = "لا يمكن حذف هذه الطلبية لأنه تم البدء في تنفيذها أو تم إكمالها.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var salesOrderToDelete = await _context.SalesOrders
                .Include(s => s.Details)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (salesOrderToDelete == null)
            {
                return NotFound();
            }

            // إعادة التحقق من الحالة قبل الحذف الفعلي
            if (salesOrderToDelete.Status != SalesOrderStatus.New)
            {
                ModelState.AddModelError(string.Empty, "لا يمكن حذف هذه الطلبية، لقد تم تغيير حالتها.");
                await OnGetAsync(id); // إعادة تحميل الصفحة مع رسالة الخطأ
                return Page();
            }

            // حذف التفاصيل أولاً ثم رأس الطلبية
            _context.SalesOrderDetails.RemoveRange(salesOrderToDelete.Details);
            _context.SalesOrders.Remove(salesOrderToDelete);

            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
