using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keswa.Models
{
    public class QualityInspection
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "المنتج")]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        [Display(Name = "العامل المسؤول")]
        public string? AssignedToId { get; set; }

        [ForeignKey("AssignedToId")]
        public ApplicationUser? AssignedTo { get; set; }

        [Required]
        [Display(Name = "الحالة")]
        public string Status { get; set; } // e.g., "Pending", "In Progress", "Completed"

        [Display(Name = "الكمية المحولة")]
        public int TransferredQuantity { get; set; }

        [Display(Name = "كمية الدرجة الأولى")]
        public int QuantityGradeA { get; set; }

        [Display(Name = "كمية الدرجة الثانية")]
        public int QuantityGradeB { get; set; }

        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "تاريخ الإكمال")]
        public DateTime? CompletedDate { get; set; }
    }
}
