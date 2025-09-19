using Keswa.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Keswa.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser,IdentityRole,string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Material> Materials { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<GoodsReceiptNote> GoodsReceiptNotes { get; set; }
        public DbSet<GoodsReceiptNoteDetail> GoodsReceiptNoteDetails { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<BillOfMaterialItem> BillOfMaterialItems { get; set; }
        public DbSet<WorkOrder> WorkOrders { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<SalesOrder> SalesOrders { get; set; }
        public DbSet<SalesOrderDetail> SalesOrderDetails { get; set; }

        public DbSet<Keswa.Models.Worker> Workers { get; set; }

        public DbSet<Keswa.Models.ProductionLog> ProductionLogs { get; set; }

        public DbSet<Keswa.Models.WorkOrderRouting> WorkOrderRoutings { get; set; }

        public DbSet<Keswa.Models.MaterialIssuanceNote> MaterialIssuanceNotes { get; set; }
        public DbSet<Keswa.Models.MaterialIssuanceNoteDetail> MaterialIssuanceNoteDetails { get; set; }

        public DbSet<Keswa.Models.DepartmentCost> DepartmentCosts { get; set; }

        public DbSet<Keswa.Models.ProductionReceiptLog> ProductionReceiptLogs { get; set; }

        public DbSet<Keswa.Models.AuditLog> AuditLogs { get; set; }

        public DbSet<Keswa.Models.MaterialRequisition> MaterialRequisitions { get; set; }
        public DbSet<Keswa.Models.MaterialRequisitionDetail> MaterialRequisitionDetails { get; set; }
        public DbSet<Keswa.Models.CuttingStatement> CuttingStatements { get; set; }

        public DbSet<Keswa.Models.Color> Colors { get; set; }

        // Sales 
        public DbSet<Keswa.Models.CustomerTransaction> CustomerTransactions { get; set; }


        // *** تم إضافة هذه الوظيفة لحل مشكلة الحذف المتعدد ***
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // هذه القاعدة تقوم بالمرور على جميع العلاقات في قاعدة البيانات
            // وتمنع الحذف التلقائي (Cascade Delete) لتجنب حدوث أي أخطاء
            foreach (var relationship in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}
