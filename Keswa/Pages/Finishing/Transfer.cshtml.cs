using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Keswa.Data;
using Keswa.Models;
using Keswa.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Keswa.Pages.Finishing
{
    public class TransferModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IQualityService _qualityService;

        public TransferModel(ApplicationDbContext context, IQualityService qualityService)
        {
            _context = context;
            _qualityService = qualityService;
        }

        [BindProperty]
        public Product Product { get; set; }

        [BindProperty]
        [Required]
        [Display(Name = "الكمية المحولة")]
        [Range(1, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من صفر")]
        public int TransferQuantity { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Product = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);

            if (Product == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _qualityService.TransferProductToQualityAsync(Product.Id, TransferQuantity);

            return RedirectToPage("./Index");
        }
    }
}
