using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Keswa.Pages.Quality
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<QualityInspection> Inspections { get; set; }

        public async Task OnGetAsync()
        {
            Inspections = await _context.QualityInspections
                .Include(q => q.Product)
                .Include(q => q.AssignedTo)
                .Where(q => q.Status != "Completed") // Do not show completed tasks
                .ToListAsync();
        }
    }
}
