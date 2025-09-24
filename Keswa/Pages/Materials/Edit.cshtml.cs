using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Keswa.Pages.Materials
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Material Material { get; set; }

        public SelectList ColorOptions { get; set; }
        public SelectList UnitOptions { get; set; }

        // تجهيز قائمة الخيارات لـ UnitOfMeasure بالعربي
        public void PopulateUnitOptions()
        {
            var values = Enum.GetValues(typeof(UnitOfMeasure))
                             .Cast<UnitOfMeasure>()
                             .Select(u => new SelectListItem
                             {
                                 Value = ((int)u).ToString(),
                                 Text = u.GetType()
                                         .GetMember(u.ToString())
                                         .First()
                                         .GetCustomAttribute<DisplayAttribute>()?
                                         .Name ?? u.ToString(),
                                 Selected = u == Material.Unit
                             }).ToList();

            UnitOptions = new SelectList(values, "Value", "Text", Material.Unit);
        }

        // عند فتح الصفحة لجلب المادة والخيارات
        public async Task<IActionResult> OnGetAsync(int id)
        {
            Material = await _context.Materials.FindAsync(id);
            if (Material == null) return NotFound();

            ColorOptions = new SelectList(_context.Colors.OrderBy(c => c.Name), "Id", "Name", Material.ColorId);
            PopulateUnitOptions();

            return Page();
        }

        // عند حفظ التعديلات
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ColorOptions = new SelectList(_context.Colors.OrderBy(c => c.Name), "Id", "Name", Material.ColorId);
                PopulateUnitOptions();
                return Page();
            }

            var materialInDb = await _context.Materials.FindAsync(Material.Id);
            if (materialInDb == null) return NotFound();

            materialInDb.Name = Material.Name;
            materialInDb.ColorId = Material.ColorId;
            materialInDb.Unit = Material.Unit;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم تعديل المادة الخام بنجاح.";

            return RedirectToPage("./Index");
        }
    }
}
