using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Keswa.Models
{
    public class PurchaseOrderDetail
    {
        public int Id { get; set; }

        public int PurchaseOrderId { get; set; }
        [ValidateNever]
        public PurchaseOrder PurchaseOrder { get; set; }

        [Required]
        [Display(Name = "المادة الخام")]
        public int MaterialId { get; set; }
        [ValidateNever]
        public Material Material { get; set; }

        [Required]
        [Display(Name = "الكمية المطلوبة")]
        [Range(0.001, double.MaxValue)]
        public double Quantity { get; set; }
    }
}
