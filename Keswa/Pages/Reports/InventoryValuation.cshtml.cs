using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.Reports
{
    public class InventoryValuationModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public InventoryValuationModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Filters
        [BindProperty(SupportsGet = true)]
        public int? SelectedWarehouseId { get; set; }

        [BindProperty(SupportsGet = true)]
        public ValuationMethod SelectedValuationMethod { get; set; } = ValuationMethod.AveragePrice;

        public SelectList WarehouseList { get; set; }

        // Report Results
        public List<InventoryValuationViewModel> ValuationReport { get; set; }
        public decimal GrandTotal { get; set; }

        public async Task OnGetAsync()
        {
            WarehouseList = new SelectList(await _context.Warehouses.OrderBy(w => w.Name).ToListAsync(), "Id", "Name");

            // *** تم التعديل هنا: فلترة النتائج لتشمل المواد الخام فقط ***
            var query = _context.InventoryItems
                .Where(i => i.ItemType == InventoryItemType.RawMaterial && i.MaterialId != null)
                .Include(i => i.Material)
                .Include(i => i.Warehouse)
                .AsQueryable();

            if (SelectedWarehouseId.HasValue)
            {
                query = query.Where(i => i.WarehouseId == SelectedWarehouseId.Value);
            }

            var inventoryItems = await query.ToListAsync();
            ValuationReport = new List<InventoryValuationViewModel>();

            foreach (var item in inventoryItems)
            {
                decimal unitPrice = 0;
                // *** تم التعديل هنا: التأكد من أن item.MaterialId له قيمة ***
                if (item.MaterialId.HasValue)
                {
                    if (SelectedValuationMethod == ValuationMethod.AveragePrice)
                    {
                        unitPrice = await GetAverageMaterialCost(item.MaterialId.Value);
                    }
                    else // Highest Price
                    {
                        unitPrice = await GetHighestMaterialCost(item.MaterialId.Value);
                    }
                }

                ValuationReport.Add(new InventoryValuationViewModel
                {
                    MaterialName = item.Material.Name,
                    WarehouseName = item.Warehouse.Name,
                    Quantity = item.StockLevel,
                    UnitPrice = unitPrice,
                    TotalValue = (decimal)item.StockLevel * unitPrice
                });
            }

            GrandTotal = ValuationReport.Sum(r => r.TotalValue);
        }

        private async Task<decimal> GetAverageMaterialCost(int materialId)
        {
            var receipts = await _context.GoodsReceiptNoteDetails
                .Where(d => d.MaterialId == materialId && d.UnitPrice > 0 && d.Quantity > 0)
                .ToListAsync();

            if (!receipts.Any()) return 0;

            var totalCost = receipts.Sum(d => (decimal)d.Quantity * d.UnitPrice);
            var totalQuantity = receipts.Sum(d => d.Quantity);

            if (totalQuantity == 0) return 0;
            return totalCost / (decimal)totalQuantity;
        }

        private async Task<decimal> GetHighestMaterialCost(int materialId)
        {
            var receipts = await _context.GoodsReceiptNoteDetails
               .Where(d => d.MaterialId == materialId && d.UnitPrice > 0)
               .ToListAsync();

            if (!receipts.Any()) return 0;

            return receipts.Max(r => r.UnitPrice);
        }
    }

    public enum ValuationMethod
    {
        [Display(Name = "متوسط السعر")]
        AveragePrice,
        [Display(Name = "أعلى سعر شراء")]
        HighestPrice
    }

    public class InventoryValuationViewModel
    {
        public string MaterialName { get; set; }
        public string WarehouseName { get; set; }
        public double Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalValue { get; set; }
    }
}
