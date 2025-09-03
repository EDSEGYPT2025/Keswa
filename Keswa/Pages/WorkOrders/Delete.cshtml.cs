using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.WorkOrders
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public WorkOrder WorkOrder { get; set; } = default!;
        public bool IsDeletable { get; set; } = true;
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            WorkOrder = await _context.WorkOrders
                .Include(w => w.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (WorkOrder == null)
            {
                return NotFound();
            }

            // التحقق إذا كان أمر الشغل مرتبطاً بأي طلبية
            var isLinkedToSalesOrder = await _context.SalesOrderDetails.AnyAsync(d => d.WorkOrderId == id);
            if (isLinkedToSalesOrder)
            {
                IsDeletable = false;
                StatusMessage = "لا يمكن حذف أمر الشغل هذا لأنه مرتبط بطلبية عميل. إذا كنت تريد حذفه، يجب أولاً تعديل الطلبية المرتبطة به.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // إعادة التحقق قبل الحذف الفعلي
            var isLinkedToSalesOrder = await _context.SalesOrderDetails.AnyAsync(d => d.WorkOrderId == id);
            if (isLinkedToSalesOrder)
            {
                // لا تقم بالحذف وأبلغ المستخدم
                await OnGetAsync(id); // إعادة تحميل الصفحة مع رسالة الخطأ
                return Page();
            }

            var workOrderToDelete = await _context.WorkOrders.FindAsync(id);

            if (workOrderToDelete != null)
            {
                _context.WorkOrders.Remove(workOrderToDelete);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
