// Keswa/Pages/Departments/_AssignSewingBatch.cshtml.cs

using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Keswa.Pages.Departments
{
    public class AssignSewingBatchModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AssignSewingBatchModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public SewingBatch SewingBatch { get; set; }
        public SelectList WorkerSelectList { get; set; }

        public class InputModel
        {
            public int SewingBatchId { get; set; }

            [Display(Name = "العامل")]
            [Required(ErrorMessage = "يجب اختيار عامل.")]
            public int WorkerId { get; set; }

            [Display(Name = "الكمية المسندة")]
            [Required(ErrorMessage = "حقل الكمية مطلوب.")]
            [Range(1, int.MaxValue, ErrorMessage = "يجب أن تكون الكمية أكبر من صفر.")]
            public int AssignedQuantity { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int sewingBatchId)
        {
            // -- تصحيح: تم تعديل جملة Include لتتبع المسار الصحيح للعلاقات --
            SewingBatch = await _context.SewingBatches
                .Include(b => b.CuttingStatement)
                    .ThenInclude(cs => cs.WorkOrder)
                        .ThenInclude(wo => wo.Product)
                .FirstOrDefaultAsync(b => b.Id == sewingBatchId);

            if (SewingBatch == null)
            {
                return NotFound();
            }

            Input = new InputModel
            {
                SewingBatchId = SewingBatch.Id,
                AssignedQuantity = SewingBatch.Quantity
            };

            var workers = await _context.Workers
                .OrderBy(w => w.Name)
                .ToListAsync();

            WorkerSelectList = new SelectList(workers, "Id", "Name");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            SewingBatch = await _context.SewingBatches.FindAsync(Input.SewingBatchId);
            if (SewingBatch == null)
            {
                return NotFound();
            }

            if (Input.AssignedQuantity > SewingBatch.Quantity)
            {
                ModelState.AddModelError("Input.AssignedQuantity", "الكمية المسندة لا يمكن أن تكون أكبر من كمية التشغيلة.");
            }

            bool assignmentExists = await _context.WorkerAssignments.AnyAsync(a =>
                a.SewingBatchId == Input.SewingBatchId &&
                a.WorkerId == Input.WorkerId &&
                a.Status != Enums.AssignmentStatus.Completed);

            if (assignmentExists)
            {
                ModelState.AddModelError(string.Empty, "يوجد بالفعل مهمة مفتوحة لنفس العامل على هذه التشغيلة.");
            }

            if (!ModelState.IsValid)
            {
                var workers = await _context.Workers.OrderBy(w => w.Name).ToListAsync();
                WorkerSelectList = new SelectList(workers, "Id", "Name");

                // -- تصحيح: يجب إعادة تحميل البيانات المرتبطة عند حدوث خطأ لعرضها في الواجهة --
                SewingBatch = await _context.SewingBatches
                    .Include(b => b.CuttingStatement)
                        .ThenInclude(cs => cs.WorkOrder)
                            .ThenInclude(wo => wo.Product)
                    .FirstOrDefaultAsync(b => b.Id == Input.SewingBatchId);
                return Page();
            }

            var newAssignment = new WorkerAssignment
            {
                SewingBatchId = Input.SewingBatchId,
                WorkerId = Input.WorkerId,
                AssignedQuantity = Input.AssignedQuantity,
                AssignedDate = DateTime.Now,
                Status = Enums.AssignmentStatus.InProgress,
                ReceivedQuantity = 0,
                ScrappedQuantity = 0
            };

            _context.WorkerAssignments.Add(newAssignment);

            SewingBatch.Status = Enums.BatchStatus.InProgress;
            _context.SewingBatches.Update(SewingBatch);

            await _context.SaveChangesAsync();

            return RedirectToPage("/Departments/Sewing");
        }
    }
}