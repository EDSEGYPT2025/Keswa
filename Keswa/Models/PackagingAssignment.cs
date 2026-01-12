using Keswa.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keswa.Models
{
    public class PackagingAssignment
    {
        public int Id { get; set; }

        [Display(Name = "تشغيلة التعبئة")]
        public int PackagingBatchId { get; set; }
        [ForeignKey("PackagingBatchId")]
        [ValidateNever]
        public PackagingBatch PackagingBatch { get; set; }

        [Display(Name = "الموظف")]
        public int WorkerId { get; set; }
        [ForeignKey("WorkerId")]
        [ValidateNever]
        public Worker Worker { get; set; }

        [Display(Name = "الكمية المسلمة للتعبئة")]
        public int AssignedQuantity { get; set; }

        [Display(Name = "الكمية المكتملة")]
        public int CompletedQuantity { get; set; } = 0;

        [NotMapped]
        [Display(Name = "الكمية المتبقية")]
        public int RemainingQuantity => AssignedQuantity - CompletedQuantity;

        [Display(Name = "الحالة")]
        public PackagingAssignmentStatus Status { get; set; } = PackagingAssignmentStatus.InProgress;

        [Display(Name = "تاريخ التكليف")]
        public DateTime AssignmentDate { get; set; } = DateTime.Now;
    }
}