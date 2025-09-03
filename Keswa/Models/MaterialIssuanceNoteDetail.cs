using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Keswa.Models
{
    public class MaterialIssuanceNoteDetail
    {
        public int Id { get; set; }

        [Required]
        public int MaterialIssuanceNoteId { get; set; }
        [ValidateNever]
        public MaterialIssuanceNote MaterialIssuanceNote { get; set; }

        [Required]
        [Display(Name = "المادة الخام")]
        public int MaterialId { get; set; }
        [ValidateNever]
        public Material Material { get; set; }

        [Required(ErrorMessage = "الكمية مطلوبة")]
        [Display(Name = "الكمية المصروفة")]
        [Range(0.01, double.MaxValue, ErrorMessage = "يجب أن تكون الكمية أكبر من صفر")]
        public double Quantity { get; set; }
    }
}
