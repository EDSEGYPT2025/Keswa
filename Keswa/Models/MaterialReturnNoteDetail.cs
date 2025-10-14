using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Keswa.Models
{
    public class MaterialReturnNoteDetail
    {
        public int Id { get; set; }
        public int MaterialReturnNoteId { get; set; }
        [ValidateNever]
        public MaterialReturnNote MaterialReturnNote { get; set; }

        [Display(Name = "الخامة")]
        public int MaterialId { get; set; }
        [ValidateNever]
        public Material Material { get; set; }

        [Display(Name = "الكمية المرتجعة")]
        public double QuantityReturned { get; set; }
    }
}