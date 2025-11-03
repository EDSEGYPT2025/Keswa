using System.Threading.Tasks;
using Keswa.Data;
using Keswa.Models;
using Microsoft.EntityFrameworkCore;

namespace Keswa.Services
{
    public class QualityService : IQualityService
    {
        private readonly ApplicationDbContext _context;

        public QualityService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task TransferProductToQualityAsync(int productId, int quantity)
        {
            var productToUpdate = await _context.Products.FindAsync(productId);
            if (productToUpdate == null) return;

            // 1. Create a record for the transfer
            var transfer = new DepartmentTransfer
            {
                ProductId = productToUpdate.Id,
                FromDepartment = "Finishing",
                ToDepartment = "Quality",
                Quantity = quantity,
                TransferDate = System.DateTime.Now
            };
            _context.DepartmentTransfers.Add(transfer);

            // 2. Update the product's department
            productToUpdate.CurrentDepartment = "Quality";
            _context.Attach(productToUpdate).State = EntityState.Modified;

            // 3. Create a quality inspection task
            var inspection = new QualityInspection
            {
                ProductId = productToUpdate.Id,
                Status = "Pending",
                TransferredQuantity = quantity,
                CreatedDate = System.DateTime.Now
            };
            _context.QualityInspections.Add(inspection);

            await _context.SaveChangesAsync();
        }

        public async Task AssignTaskToWorkerAsync(int inspectionId, string workerId)
        {
            var inspectionToUpdate = await _context.QualityInspections.FindAsync(inspectionId);
            if (inspectionToUpdate == null) return;

            inspectionToUpdate.AssignedToId = workerId;
            inspectionToUpdate.Status = "In Progress";
            _context.Attach(inspectionToUpdate).State = EntityState.Modified;

            await _context.SaveChangesAsync();
        }

        public async Task ReceiveInspectionResultAsync(int inspectionId, int quantityGradeA, int quantityGradeB)
        {
            var inspectionToUpdate = await _context.QualityInspections.FindAsync(inspectionId);
            if (inspectionToUpdate == null) return;

            inspectionToUpdate.QuantityGradeA = quantityGradeA;
            inspectionToUpdate.QuantityGradeB = quantityGradeB;
            inspectionToUpdate.Status = "Completed";
            inspectionToUpdate.CompletedDate = System.DateTime.Now;
            _context.Attach(inspectionToUpdate).State = EntityState.Modified;

            await _context.SaveChangesAsync();
        }
    }
}
