using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.Costs
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public List<DepartmentCost> DepartmentCosts { get; set; } = new();

        public async Task OnGetAsync()
        {
            var existingCosts = await _context.DepartmentCosts.ToDictionaryAsync(dc => dc.Department, dc => dc);
            var allDepartments = Enum.GetValues(typeof(Department)).Cast<Department>();

            foreach (var dept in allDepartments)
            {
                if (existingCosts.TryGetValue(dept, out var cost))
                {
                    DepartmentCosts.Add(cost);
                }
                else
                {
                    DepartmentCosts.Add(new DepartmentCost { Department = dept, CostPerUnit = 0 });
                }
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            foreach (var cost in DepartmentCosts)
            {
                var existingCost = await _context.DepartmentCosts.FirstOrDefaultAsync(dc => dc.Department == cost.Department);
                if (existingCost != null)
                {
                    existingCost.CostPerUnit = cost.CostPerUnit;
                }
                else
                {
                    // فقط أضف التكاليف التي لها قيمة
                    if (cost.CostPerUnit > 0)
                    {
                        _context.DepartmentCosts.Add(cost);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}
