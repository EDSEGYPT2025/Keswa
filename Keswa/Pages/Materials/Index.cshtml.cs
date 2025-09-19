using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.Materials
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context) { _context = context; }

        public IList<Material> Materials { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        public async Task OnGetAsync()
        {
            // *** تم التعديل هنا: إضافة Include لجلب بيانات اللون المرتبطة ***
            var query = _context.Materials.Include(m => m.Color).AsQueryable();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(m => m.Name.Contains(SearchTerm));
            }

            var totalRecords = await query.CountAsync();
            TotalPages = (int)System.Math.Ceiling(totalRecords / (double)PageSize);

            Materials = await query.OrderBy(m => m.Name)
                                    .Skip((CurrentPage - 1) * PageSize)
                                    .Take(PageSize)
                                    .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var material = await _context.Materials.FindAsync(id);

            if (material != null)
            {
                _context.Materials.Remove(material);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"تم حذف المادة الخام بنجاح.";
            }
            return RedirectToPage();
        }
    }
}

