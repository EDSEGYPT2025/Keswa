using Keswa.Enums;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic; // تأكد من وجود هذا الـ using

namespace Keswa.Models
{
    public class PackagingBatch
    {
        public int Id { get; set; }

        [Display(Name = "رقم تشغيلة التعبئة")]
        public string PackagingBatchNumber { get; set; }

        public int QualityBatchId { get; set; }
        public QualityBatch QualityBatch { get; set; }

        public int WorkOrderId { get; set; }
        public WorkOrder WorkOrder { get; set; }

        [Display(Name = "الكمية الكلية")]
        public int Quantity { get; set; }

        public PackagingBatchStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // --- أضف هذا السطر المفقود هنا لإنهاء الخطأ ---
        public ICollection<PackagingAssignment> PackagingAssignments { get; set; }
    }
}