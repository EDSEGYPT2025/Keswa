using Keswa.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Keswa.Models
{
    public class QualityBatch
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "رقم تشغيلة الجودة")]
        public string QualityBatchNumber { get; set; }

        [Display(Name = "تشغيلة التشطيب الأصلية")]
        public int FinishingBatchId { get; set; }
        [ValidateNever]
        [ForeignKey("FinishingBatchId")]
        public FinishingBatch FinishingBatch { get; set; }

        [Display(Name = "أمر الشغل")]
        public int WorkOrderId { get; set; }
        [ValidateNever]
        [ForeignKey("WorkOrderId")]
        public WorkOrder WorkOrder { get; set; }

        [Display(Name = "الكمية المستلمة")]
        public int Quantity { get; set; }

        [Display(Name = "الحالة")]
        public QualityBatchStatus Status { get; set; }

        [Display(Name = "تاريخ الإنشاء")]
        public System.DateTime CreatedAt { get; set; }

        [ValidateNever]
        public ICollection<QualityAssignment> QualityAssignments { get; set; }
    }
}
