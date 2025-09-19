using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.WorkOrders
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public WorkOrder WorkOrder { get; set; } = new();

        public SelectList ProductList { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            ProductList = new SelectList(await _context.Products.OrderBy(p => p.Name).ToListAsync(), "Id", "Name");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ProductList = new SelectList(await _context.Products.OrderBy(p => p.Name).ToListAsync(), "Id", "Name");
                return Page();
            }

            // الخطوة 1: حفظ أمر الشغل أولاً لتوليد ID
            _context.WorkOrders.Add(WorkOrder);
            await _context.SaveChangesAsync();

            // الخطوة 2: توليد وحفظ رقم أمر الشغل
            var countForYear = await _context.WorkOrders.CountAsync(wo => wo.CreationDate.Year == WorkOrder.CreationDate.Year);
            WorkOrder.WorkOrderNumber = $"WO-{WorkOrder.CreationDate.Year}-{countForYear}";

            // *** تم التعديل هنا: إنشاء مسار الإنتاج تلقائياً ***
            // الخطوة 3: إنشاء جميع مراحل الإنتاج لأمر الشغل الجديد
            var departments = Enum.GetValues(typeof(Department)).Cast<Department>();
            foreach (var dept in departments)
            {
                _context.WorkOrderRoutings.Add(new WorkOrderRouting
                {
                    WorkOrderId = WorkOrder.Id,
                    Department = dept,
                    Status = WorkOrderStageStatus.Pending // الحالة الافتراضية هي "قيد الانتظار"
                });
            }

            // الخطوة 4: حفظ جميع التغييرات النهائية
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"تم إنشاء أمر الشغل رقم {WorkOrder.WorkOrderNumber} بنجاح.";
            return RedirectToPage("./Index");
        }
    }
}
