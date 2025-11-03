using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Keswa.Data;
using Keswa.Models;
using Keswa.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Keswa.Pages.Quality
{
    public class ReceiveModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IQualityService _qualityService;


        public ReceiveModel(ApplicationDbContext context, IQualityService qualityService)
        {
            _context = context;
            _qualityService = qualityService;
        }

        [BindProperty]
        public QualityInspection Inspection { get; set; }

        [BindProperty]
        [Display(Name = "كمية الدرجة الأولى")]
        [Range(0, int.MaxValue)]
        public int QuantityGradeA { get; set; }

        [BindProperty]
        [Display(Name = "كمية الدرجة الثانية")]
        [Range(0, int.MaxValue)]
        public int QuantityGradeB { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Inspection = await _context.QualityInspections
                .Include(q => q.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Inspection == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var inspectionToUpdate = await _context.QualityInspections.FindAsync(Inspection.Id);
            if (inspectionToUpdate == null)
            {
                return NotFound();
            }

            if (QuantityGradeA + QuantityGradeB != inspectionToUpdate.TransferredQuantity)
            {
                ModelState.AddModelError(string.Empty, "مجموع كميات الدرجة الأولى والثانية يجب أن يساوي الكمية المحولة.");
            }

            if (!ModelState.IsValid)
            {
                // Re-fetch the product info in case of an error to display the page correctly
                Inspection.Product = await _context.Products.FindAsync(inspectionToUpdate.ProductId);
                return Page();
            }

            await _qualityService.ReceiveInspectionResultAsync(Inspection.Id, QuantityGradeA, QuantityGradeB);

            return RedirectToPage("./Index");
        }
    }
}
