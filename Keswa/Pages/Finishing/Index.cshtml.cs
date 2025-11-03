using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Keswa.Pages.Finishing
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Product> Products { get; set; }

        public async Task OnGetAsync()
        {
            Products = await _context.Products
                .Where(p => p.CurrentDepartment == "Finishing")
                .ToListAsync();
        }
    }
}
