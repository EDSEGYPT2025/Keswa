using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Keswa.Pages.Departments
{
    public class ReceiveProductionModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ReceiveProductionModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public WorkerAssignment WorkerAssignment { get; set; }

        public class InputModel
        {
            public int WorkerAssignmentId { get; set; }

            [Display(Name = "الكمية المستلمة (سليمة)")]
            [Required(ErrorMessage = "هذا الحقل مطلوب.")]
            [Range(0, int.MaxValue)]
            public int ReceivedQuantity { get; set; }

            [Display(Name = "الكمية الهالكة")]
            [Required(ErrorMessage = "هذا الحقل مطلوب.")]
            [Range(0, int.MaxValue)]
            public int ScrappedQuantity { get; set; } = 0; // القيمة الافتراضية صفر
        }

        public async Task<IActionResult> OnGetAsync(int assignmentId)
        {
            WorkerAssignment = await _context.WorkerAssignments
                .Include(wa => wa.Worker)
                .Include(wa => wa.SewingBatch.CuttingStatement.WorkOrder.Product)
                .FirstOrDefaultAsync(wa => wa.Id == assignmentId);

            if (WorkerAssignment == null) return NotFound();

            Input.WorkerAssignmentId = assignmentId;
            // اقتراح الكمية المتبقية تلقائياً
            Input.ReceivedQuantity = WorkerAssignment.RemainingQuantity;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // --- (الكود الحالي لجلب البيانات والتحقق من المدخلات يبقى كما هو) ---
            if (!ModelState.IsValid)
            {
                // يجب إعادة تحميل البيانات اللازمة للعرض في حالة وجود خطأ
                WorkerAssignment = await _context.WorkerAssignments
                    .Include(wa => wa.Worker)
                    .Include(wa => wa.SewingBatch.CuttingStatement.WorkOrder.Product)
                    .FirstOrDefaultAsync(wa => wa.Id == Input.WorkerAssignmentId);
                return Page();
            }

            var workerAssignment = await _context.WorkerAssignments
                .Include(wa => wa.SewingBatch) // مهم جداً لتضمين التشغيلة الرئيسية
                .FirstOrDefaultAsync(wa => wa.Id == Input.WorkerAssignmentId);

            if (workerAssignment == null) return NotFound();

            if ((Input.ReceivedQuantity + Input.ScrappedQuantity) > workerAssignment.AssignedQuantity)
            {
                ModelState.AddModelError("", "مجموع الكميات السليمة والهالكة أكبر من الكمية المسلمة للعامل.");
                // إعادة تحميل البيانات
                WorkerAssignment = await _context.WorkerAssignments
                    .Include(wa => wa.Worker)
                    .Include(wa => wa.SewingBatch.CuttingStatement.WorkOrder.Product)
                    .FirstOrDefaultAsync(wa => wa.Id == Input.WorkerAssignmentId);
                return Page();
            }

            // 1. تحديث بيانات تشغيلة العامل
            workerAssignment.ReceivedQuantity = Input.ReceivedQuantity;
            workerAssignment.ScrappedQuantity = Input.ScrappedQuantity;
            workerAssignment.Status = AssignmentStatus.Completed;

            // ================== بداية الكود الجديد ==================

            // 2. التحقق مما إذا كانت التشغيلة الرئيسية قد اكتملت
            var sewingBatch = workerAssignment.SewingBatch;
            var allAssignmentsForBatch = await _context.WorkerAssignments
                .Where(wa => wa.SewingBatchId == sewingBatch.Id)
                .ToListAsync();

            // نتأكد أن كل التشغيلات الفرعية التابعة للتشغيلة الرئيسية قد اكتملت
            var isBatchComplete = allAssignmentsForBatch.All(a => a.Status == AssignmentStatus.Completed);

            if (isBatchComplete)
            {
                // إذا اكتملت، نغير حالة التشغيلة الرئيسية إلى "مكتملة"
                sewingBatch.Status = BatchStatus.Completed;
            }

            // ================== نهاية الكود الجديد ==================

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم استلام الإنتاج من العامل بنجاح.";
            return RedirectToPage("/Departments/Sewing");
        }
    }
}