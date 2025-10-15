using Keswa.Data;
using Keswa.Enums;
using Keswa.Helpers;
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
    public class SewingModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public SewingModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // خصائص لعرض البيانات في الصفحة الرئيسية
        public List<SewingBatchViewModel> PendingBatches { get; set; } = new();
        public List<WorkerAssignmentViewModel> AssignmentsInProgress { get; set; } = new();
        public List<CompletedAssignmentViewModel> CompletedAssignments { get; set; } = new();

        // خصائص خاصة بالشاشة المنبثقة (Modal)
        [BindProperty]
        public AssignmentInputModel AssignmentInput { get; set; }
        public SewingBatch SelectedSewingBatch { get; set; }
        public SelectList WorkerList { get; set; }
        public int RemainingQuantityInBatch { get; set; }

        // نموذج لإدخال البيانات من المودال
        public class AssignmentInputModel
        {
            [Required]
            public int SewingBatchId { get; set; }
            [Required(ErrorMessage = "يجب اختيار العامل")]
            [Display(Name = "العامل")]
            public int WorkerId { get; set; }
            [Required(ErrorMessage = "يجب إدخال الكمية")]
            [Display(Name = "الكمية المسلمة للعامل")]
            [Range(1, int.MaxValue, ErrorMessage = "يجب أن تكون الكمية أكبر من صفر")]
            public int AssignedQuantity { get; set; }
        }

        public async Task OnGetAsync()
        {
            PendingBatches = await _context.SewingBatches
                .Include(sb => sb.CuttingStatement.WorkOrder.Product)
                .Where(sb => sb.Status == BatchStatus.PendingTransfer)
                .Select(sb => new SewingBatchViewModel
                {
                    SewingBatchId = sb.Id,
                    SewingBatchNumber = sb.SewingBatchNumber,
                    ProductName = sb.CuttingStatement.WorkOrder.Product.Name,
                    Quantity = sb.Quantity
                })
                .ToListAsync();

            AssignmentsInProgress = await _context.WorkerAssignments
                .Include(wa => wa.Worker)
                .Include(wa => wa.SewingBatch.CuttingStatement.WorkOrder)
                .Where(wa => wa.Status == AssignmentStatus.InProgress)
                .Select(wa => new WorkerAssignmentViewModel
                {
                    AssignmentId = wa.Id,
                    WorkerName = wa.Worker.Name,
                    SewingBatchNumber = wa.SewingBatch.SewingBatchNumber,
                    AssignedQuantity = wa.AssignedQuantity,
                    ReceivedQuantity = wa.ReceivedQuantity,
                    ScrappedQuantity = wa.ScrappedQuantity
                })
                .ToListAsync();
        }

        // --- دالة جديدة لجلب بيانات المودال ---
        public async Task<PartialViewResult> OnGetAssignBatchModal(int sewingBatchId)
        {
            SelectedSewingBatch = await _context.SewingBatches
                .Include(sb => sb.CuttingStatement.WorkOrder.Product)
                .FirstOrDefaultAsync(sb => sb.Id == sewingBatchId);

            var totalAssigned = await _context.WorkerAssignments
                .Where(wa => wa.SewingBatchId == sewingBatchId)
                .SumAsync(wa => wa.AssignedQuantity);

            RemainingQuantityInBatch = SelectedSewingBatch.Quantity - totalAssigned;

            var workers = await _context.Workers
                .Where(w => w.Department == Department.Sewing)
                .OrderBy(w => w.Name)
                .ToListAsync();

            WorkerList = new SelectList(workers, "Id", "Name");
            AssignmentInput = new AssignmentInputModel { SewingBatchId = sewingBatchId };

            // إرجاع الواجهة الجزئية مع البيانات
            return Partial("_AssignSewingBatch", this);
        }

        // --- دالة جديدة لحفظ بيانات المودال ---
        public async Task<IActionResult> OnPostAssignBatchAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "حدث خطأ في البيانات المدخلة. يرجى المحاولة مرة أخرى.";
                return RedirectToPage();
            }

            var sewingBatch = await _context.SewingBatches.FindAsync(AssignmentInput.SewingBatchId);
            if (sewingBatch != null)
            {
                sewingBatch.Status = BatchStatus.Transferred;
            }

            var workerAssignment = new WorkerAssignment
            {
                SewingBatchId = AssignmentInput.SewingBatchId,
                WorkerId = AssignmentInput.WorkerId,
                AssignedQuantity = AssignmentInput.AssignedQuantity,
                AssignedDate = DateTimeHelper.EgyptNow
            };

            _context.WorkerAssignments.Add(workerAssignment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم تسليم التشغيلة للعامل بنجاح.";
            return RedirectToPage();
        }
    }

    // ViewModels
    public class SewingBatchViewModel
    {
        public int SewingBatchId { get; set; }
        public string SewingBatchNumber { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }

    public class WorkerAssignmentViewModel
    {
        public int AssignmentId { get; set; }
        public string WorkerName { get; set; }
        public string SewingBatchNumber { get; set; }
        public int AssignedQuantity { get; set; }
        public int ReceivedQuantity { get; set; }
        public int ScrappedQuantity { get; set; }
        public int RemainingQuantity => AssignedQuantity - (ReceivedQuantity + ScrappedQuantity);
    }

    public class CompletedAssignmentViewModel { }
}