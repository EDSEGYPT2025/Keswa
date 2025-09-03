using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Keswa.Models;

public class BillOfMaterialItem
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    [ValidateNever]
    public Product Product { get; set; }

    [Required(ErrorMessage = "يجب اختيار المادة الخام")]
    [Display(Name = "المادة الخام")]
    public int MaterialId { get; set; }
    [ValidateNever]
    public Material Material { get; set; }

    [Required(ErrorMessage = "الكمية مطلوبة")]
    [Display(Name = "الكمية المطلوبة")]
    [Range(0.001, double.MaxValue, ErrorMessage = "يجب أن تكون الكمية أكبر من صفر")]
    public double Quantity { get; set; }
}
