using Keswa.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keswa.Models
{
    public class QualityAssignment
    {
        public int Id { get; set; }

        [Display(Name = "تشغيلة الجودة")]
        public int QualityBatchId { get; set; }
        [ForeignKey("QualityBatchId")]
        [ValidateNever]
        public QualityBatch QualityBatch { get; set; }

        [Display(Name = "العامل")]
        public int WorkerId { get; set; }
        [ForeignKey("WorkerId")]
        [ValidateNever]
        public Worker Worker { get; set; }

        [Display(Name = "الكمية المسلمة")]
        public int AssignedQuantity { get; set; }

        [Display(Name = "كمية الدرجة الأولى")]
        public int ReceivedQuantityGradeA { get; set; } = 0;

        [Display(Name = "كمية الدرجة الثانية")]
        public int ReceivedQuantityGradeB { get; set; } = 0;

        [NotMapped]
        public int TotalReceived => ReceivedQuantityGradeA + ReceivedQuantityGradeB;

        [NotMapped]
        [Display(Name = "الكمية المتبقية")]
        public int RemainingQuantity => AssignedQuantity - TotalReceived;

        [Display(Name = "الحالة")]
        public QualityAssignmentStatus Status { get; set; } = QualityAssignmentStatus.InProgress;

        [Display(Name = "تاريخ التسليم")]
        public System.DateTime AssignmentDate { get; set; } = System.DateTime.Now;
    }
}
