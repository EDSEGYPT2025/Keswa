using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.Departments
{
    public class AssignFinishingBatchModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AssignFinishingBatchModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public FinishingBatch BatchToAssign { get; set; }
        public SelectList Workers { get; set; }

        [BindProperty]
        public AssignmentInputModel Input { get; set; }

        public class AssignmentInputModel
        {
            public int FinishingBatchId { get; set; }

            [Required(ErrorMessage = "الرجاء اختيار عامل")]
            [Display(Name = "العامل")]
            public int WorkerId { get; set; }

            // -- BEGIN MODIFICATION --
            // سنقوم بتعيين هذه القيمة تلقائيًا
            [Required]
            public int AssignedQuantity { get; set; }
            // -- END MODIFICATION --

            [Required(ErrorMessage = "الرجاء تحديد نوع التسليم")]
            [Display(Name = "نوع التسليم")]
            public AssignmentType AssignmentType { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int finishingBatchId)
        {
            BatchToAssign = await _context.FinishingBatches
                .Include(b => b.WorkOrder).ThenInclude(wo => wo.Product)
                .FirstOrDefaultAsync(b => b.Id == finishingBatchId);

            if (BatchToAssign == null || BatchToAssign.Status != FinishingBatchStatus.Pending)
            {
                TempData["ErrorMessage"] = "التشغيلة غير صالحة للتسليم.";
                return RedirectToPage("./Finishing");
            }

            Workers = new SelectList(await _context.Workers.OrderBy(w => w.Name).ToListAsync(), "Id", "Name");

            // -- BEGIN MODIFICATION --
            // تعيين الكمية الكاملة تلقائيًا
            Input = new AssignmentInputModel
            {
                FinishingBatchId = finishingBatchId,
                AssignedQuantity = BatchToAssign.Quantity
            };
            // -- END MODIFICATION --

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // -- BEGIN MODIFICATION --
            // جلب بيانات الصفحة مرة أخرى في حال حدوث خطأ
            if (!ModelState.IsValid)
            {
                await LoadGetPrerequisites(Input.FinishingBatchId);
                return Page();
            }
            // -- END MODIFICATION --

            var batch = await _context.FinishingBatches
                .Include(b => b.WorkOrder).ThenInclude(wo => wo.Product)
                .FirstOrDefaultAsync(b => b.Id == Input.FinishingBatchId);

            if (batch == null || batch.Status != FinishingBatchStatus.Pending)
            {
                TempData["ErrorMessage"] = "لا يمكن تسليم هذه التشغيلة.";
                return RedirectToPage("./Finishing");
            }

            // -- BEGIN MODIFICATION --
            // تم حذف التحقق من الكمية لأنها أصبحت تلقائية
            // -- END MODIFICATION --

            var assignment = new FinishingAssignment
            {
                FinishingBatchId = batch.Id,
                WorkerId = Input.WorkerId,
                AssignedQuantity = Input.AssignedQuantity, // القيمة الكاملة من الحقل المخفي
                RemainingQuantity = Input.AssignedQuantity,
                AssignmentType = Input.AssignmentType,
                Rate = (Input.AssignmentType == AssignmentType.External) ? batch.WorkOrder.Product.FinishingFee : 0
            };

            _context.FinishingAssignments.Add(assignment);
            batch.Status = FinishingBatchStatus.InProgress;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"تم تسليم كامل الكمية {Input.AssignedQuantity} إلى العامل بنجاح.";
            return RedirectToPage("./Finishing");
        }

        // -- BEGIN MODIFICATION --
        // دالة مساعدة لإعادة تحميل البيانات عند فشل الحفظ
        private async Task LoadGetPrerequisites(int finishingBatchId)
        {
            BatchToAssign = await _context.FinishingBatches
                .Include(b => b.WorkOrder).ThenInclude(wo => wo.Product)
                .FirstOrDefaultAsync(b => b.Id == finishingBatchId);

            Workers = new SelectList(await _context.Workers.OrderBy(w => w.Name).ToListAsync(), "Id", "Name");
        }
        // -- END MODIFICATION --
    }
}