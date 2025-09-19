using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Keswa.Pages.Colors
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context) { _context = context; }

        public IList<Color> Colors { get; set; }

        [BindProperty]
        public Color NewColor { get; set; }

        public async Task OnGetAsync()
        {
            Colors = await _context.Colors.ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Colors = await _context.Colors.ToListAsync();
                return Page();
            }

            _context.Colors.Add(NewColor);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "تمت إضافة اللون بنجاح.";
            return RedirectToPage();
        }
    }
}

