using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Keswa.Models
{
    public class MaterialRequisitionDetail
    {
        public int Id { get; set; }

        [Required]
        public int MaterialRequisitionId { get; set; }
        [ValidateNever]
        public MaterialRequisition MaterialRequisition { get; set; }

        [Required]
        [Display(Name = "المادة الخام")]
        public int MaterialId { get; set; }
        [ValidateNever]
        public Material Material { get; set; }

        [Required]
        [Display(Name = "الكمية المطلوبة")]
        [Range(0.01, double.MaxValue)]
        public double Quantity { get; set; }
    }
}