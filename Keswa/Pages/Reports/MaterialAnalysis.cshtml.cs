using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.Reports
{
    public class MaterialAnalysisModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public MaterialAnalysisModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int? SelectedMaterialId { get; set; }

        public SelectList MaterialList { get; set; }
        public MaterialAnalysisViewModel? Report { get; set; }

        public async Task OnGetAsync()
        {
            MaterialList = new SelectList(await _context.Materials.OrderBy(m => m.Name).ToListAsync(), "Id", "Name");

            if (SelectedMaterialId.HasValue)
            {
                await GenerateReport(SelectedMaterialId.Value);
            }
        }

        private async Task GenerateReport(int materialId)
        {
            var material = await _context.Materials.FindAsync(materialId);
            if (material == null) return;

            var receipts = await _context.GoodsReceiptNoteDetails
                .Where(d => d.MaterialId == materialId && d.UnitPrice > 0)
                .Include(d => d.GoodsReceiptNote)
                .OrderByDescending(d => d.GoodsReceiptNote.ReceiptDate)
                .ToListAsync();

            Report = new MaterialAnalysisViewModel
            {
                MaterialName = material.Name,
                Receipts = receipts.Select(r => new ReceiptPriceDetail
                {
                    ReceiptNumber = r.GoodsReceiptNote.Id.ToString(), // Assuming GRN Id is the number for simplicity
                    ReceiptDate = r.GoodsReceiptNote.ReceiptDate,
                    UnitPrice = r.UnitPrice
                }).ToList()
            };

            if (receipts.Any())
            {
                Report.AveragePrice = receipts.Average(r => r.UnitPrice);
                Report.HighestPrice = receipts.Max(r => r.UnitPrice);
                Report.LowestPrice = receipts.Min(r => r.UnitPrice);
            }
        }
    }

    // ViewModels for the report
    public class MaterialAnalysisViewModel
    {
        public string MaterialName { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal HighestPrice { get; set; }
        public decimal LowestPrice { get; set; }
        public List<ReceiptPriceDetail> Receipts { get; set; } = new();
    }

    public class ReceiptPriceDetail
    {
        public string ReceiptNumber { get; set; }
        public System.DateTime ReceiptDate { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
