// Keswa/Pages/Departments/_ReceiveFromWorkerModal.cshtml.cs

using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Keswa.Pages.Departments
{
    public class ReceiveFromWorkerModalModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ReceiveFromWorkerModalModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public WorkerAssignment Assignment { get; set; }

        public class InputModel
        {
            public int AssignmentId { get; set; }

            [Display(Name = "الكمية المستلمة")]
            [Required(ErrorMessage = "حقل الكمية المستلمة مطلوب.")]
            [Range(0, int.MaxValue, ErrorMessage = "يجب أن تكون الكمية المستلمة أكبر من أو تساوي صفر.")]
            public int ReceivedQuantity { get; set; }

            [Display(Name = "الكمية التالفة (سكراب)")]
            [Range(0, int.MaxValue, ErrorMessage = "يجب أن تكون الكمية التالفة أكبر من أو تساوي صفر.")]
            public int ScrappedQuantity { get; set; }

            [Display(Name = "سبب التالف")]
            public string ScrapReason { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int assignmentId)
        {
            Assignment = await _context.WorkerAssignments
                .Include(a => a.Worker)
                .Include(a => a.SewingBatch)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (Assignment == null)
            {
                return NotFound();
            }

            Input = new InputModel
            {
                AssignmentId = Assignment.Id
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Assignment = await _context.WorkerAssignments
                .FirstOrDefaultAsync(a => a.Id == Input.AssignmentId);

            if (Assignment == null)
            {
                return NotFound();
            }

            if (Input.ReceivedQuantity + Input.ScrappedQuantity > Assignment.RemainingQuantity)
            {
                ModelState.AddModelError(string.Empty, "مجموع الكمية المستلمة والتالفة لا يمكن أن يتجاوز الكمية المتبقية لدى العامل.");
            }

            if (!ModelState.IsValid)
            {
                // نحتاج لإعادة تحميل البيانات للعرض في حال وجود خطأ
                Assignment = await _context.WorkerAssignments
                    .Include(a => a.Worker)
                    .Include(a => a.SewingBatch)
                    .FirstOrDefaultAsync(a => a.Id == Input.AssignmentId);
                return Page();
            }

            // -- تصحيح: يتم تحديث الكميات المستلمة والتالفة بدلاً من المتبقية --
            Assignment.ReceivedQuantity += Input.ReceivedQuantity;
            Assignment.ScrappedQuantity += Input.ScrappedQuantity;

            // تحديث حالة المهمة إذا اكتملت (عندما تصبح المتبقية صفراً)
            if (Assignment.RemainingQuantity == 0)
            {
                Assignment.Status = Enums.AssignmentStatus.Completed;
            }

            _context.WorkerAssignments.Update(Assignment);

            // يمكنك إضافة سجلات الإنتاج والتوالف هنا كما في المثال السابق إذا أردت

            await _context.SaveChangesAsync();

            return RedirectToPage("/Departments/Sewing");
        }
    }
}