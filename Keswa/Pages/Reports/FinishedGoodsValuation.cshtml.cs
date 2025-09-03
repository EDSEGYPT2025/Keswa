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
    public class FinishedGoodsValuationModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public FinishedGoodsValuationModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Filters
        [BindProperty(SupportsGet = true)]
        public int? SelectedWarehouseId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedProductId { get; set; }

        public SelectList WarehouseList { get; set; }
        public SelectList ProductList { get; set; }

        // Report Results
        public List<FinishedGoodValuationViewModel> ValuationReport { get; set; }
        public decimal GrandTotalValue { get; set; }

        public async Task OnGetAsync()
        {
            WarehouseList = new SelectList(
                await _context.Warehouses.Where(w => w.Type == WarehouseType.FinishedGoods).OrderBy(w => w.Name).ToListAsync(),
                "Id", "Name");

            ProductList = new SelectList(
                await _context.Products.OrderBy(p => p.Name).ToListAsync(),
                "Id", "Name");

            var query = _context.InventoryItems
                .Where(i => i.ItemType == InventoryItemType.FinishedGood && i.ProductId != null && i.StockLevel > 0)
                .Include(i => i.Product)
                .Include(i => i.Warehouse)
                .AsQueryable();

            if (SelectedWarehouseId.HasValue)
            {
                query = query.Where(i => i.WarehouseId == SelectedWarehouseId.Value);
            }
            if (SelectedProductId.HasValue)
            {
                query = query.Where(i => i.ProductId == SelectedProductId.Value);
            }

            var inventoryItems = await query.ToListAsync();
            ValuationReport = new List<FinishedGoodValuationViewModel>();

            foreach (var item in inventoryItems)
            {
                var costs = await _context.ProductionReceiptLogs
                    .Where(p => p.ProductId == item.ProductId)
                    .Select(p => p.UnitCost)
                    .ToListAsync();

                ValuationReport.Add(new FinishedGoodValuationViewModel
                {
                    ProductName = item.Product.Name,
                    WarehouseName = item.Warehouse.Name,
                    Quantity = item.StockLevel,
                    AverageCost = costs.Any() ? costs.Average() : 0,
                    HighestCost = costs.Any() ? costs.Max() : 0,
                    LowestCost = costs.Any() ? costs.Min() : 0,
                    TotalValue = (decimal)item.StockLevel * (costs.Any() ? costs.Average() : 0)
                });
            }

            GrandTotalValue = ValuationReport.Sum(r => r.TotalValue);
        }
    }

    public class FinishedGoodValuationViewModel
    {
        public string ProductName { get; set; }
        public string WarehouseName { get; set; }
        public double Quantity { get; set; }
        public decimal AverageCost { get; set; }
        public decimal HighestCost { get; set; }
        public decimal LowestCost { get; set; }
        public decimal TotalValue { get; set; }
    }
}
