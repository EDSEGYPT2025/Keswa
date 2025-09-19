using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.Materials
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public CreateModel(ApplicationDbContext context) { _context = context; }

        [BindProperty]
        public Material Material { get; set; }
        public SelectList ColorList { get; set; }

        public async Task OnGetAsync()
        {
            ColorList = new SelectList(await _context.Colors.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ColorList = new SelectList(await _context.Colors.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
                return Page();
            }

            _context.Materials.Add(Material);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "تمت إضافة المادة الخام بنجاح.";
            return RedirectToPage("./Index");
        }
    }
}

