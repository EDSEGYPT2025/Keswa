using Keswa.Helpers;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keswa.Models
{
    public class FinishingProductionLog
    {
        public int Id { get; set; }

        [Display(Name = "عهدة العامل")]
        public int FinishingAssignmentId { get; set; }
        [ValidateNever]
        [ForeignKey("FinishingAssignmentId")]
        public FinishingAssignment FinishingAssignment { get; set; }

        [Display(Name = "الكمية المنتجة")]
        public int QuantityProduced { get; set; }

        [Display(Name = "سعر القطعة (فيا)")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Rate { get; set; }

        [Display(Name = "إجمالي المستحق")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalPay { get; set; }

        [Display(Name = "تاريخ التسجيل")]
        public DateTime LogDate { get; set; } = DateTimeHelper.EgyptNow;
    }
}