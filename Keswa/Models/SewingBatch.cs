using Keswa.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keswa.Models
{
    public class SewingBatch
    {
        public int Id { get; set; }

        [Display(Name = "رقم تشغيلة الخياطة")]
        public string SewingBatchNumber { get; set; }

        [Display(Name = "بيان القص المصدر")]
        public int CuttingStatementId { get; set; }
        [ValidateNever]
        [ForeignKey("CuttingStatementId")]
        public CuttingStatement CuttingStatement { get; set; }

        [Display(Name = "الكمية المحولة")]
        public int Quantity { get; set; }

        [Display(Name = "الحالة")]
        public BatchStatus Status { get; set; } = BatchStatus.PendingTransfer; // سنستخدم نفس الـ Enum

        // هذا السطر يعرف أن كل تشغيلة خياطة رئيسية يمكن أن تحتوي على
        // مجموعة من التشغيلات الفرعية المسلمة للعمال
        public virtual ICollection<WorkerAssignment> WorkerAssignments { get; set; }
    }
}