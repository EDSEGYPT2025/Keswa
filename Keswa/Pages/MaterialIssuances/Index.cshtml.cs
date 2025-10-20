using Keswa.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.MaterialIssuances
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<IssuanceNoteViewModel> IssuanceNotes { get; set; } = new List<IssuanceNoteViewModel>();

        public async Task OnGetAsync()
        {
            var allIssuances = await _context.MaterialIssuanceNotes
                .Include(i => i.WorkOrder)
                .Include(i => i.MaterialRequisition)
                .OrderByDescending(i => i.IssuanceDate)
                .ToListAsync();

            var workOrderIds = allIssuances.Select(i => i.WorkOrderId).ToList();

            var salesOrderDetails = await _context.SalesOrderDetails
                .Include(sod => sod.SalesOrder.Customer)
                .Where(sod => sod.WorkOrderId.HasValue && workOrderIds.Contains(sod.WorkOrderId.Value))
                .ToDictionaryAsync(sod => sod.WorkOrderId.Value);

            IssuanceNotes = allIssuances.Select(issuance => new IssuanceNoteViewModel
            {
                Id = issuance.Id,
                // هذا السطر سيعمل الآن بشكل صحيح
                IssuanceNoteNumber = issuance.MaterialRequisition?.MaterialRequisitionNumber,
                WorkOrderNumber = issuance.WorkOrder?.WorkOrderNumber,
                IssuanceDate = issuance.IssuanceDate,
                SalesOrderNumber = salesOrderDetails.ContainsKey(issuance.WorkOrderId) ? salesOrderDetails[issuance.WorkOrderId].SalesOrder.OrderNumber : "N/A",
                CustomerName = salesOrderDetails.ContainsKey(issuance.WorkOrderId) ? salesOrderDetails[issuance.WorkOrderId].SalesOrder.Customer.Name : "N/A"
            }).ToList();
        }

        public class IssuanceNoteViewModel
        {
            public int Id { get; set; }
            public string IssuanceNoteNumber { get; set; }
            public string SalesOrderNumber { get; set; }
            public string WorkOrderNumber { get; set; }
            public string CustomerName { get; set; }
            public DateTime IssuanceDate { get; set; }
        }
    }
}