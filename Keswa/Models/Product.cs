using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Keswa.Models;

public class Product
{
    public Product()
    {
        BillOfMaterialItems = new HashSet<BillOfMaterialItem>();
    }

    public int Id { get; set; }

    [Required(ErrorMessage = "كود الموديل مطلوب")]
    [StringLength(50)]
    [Display(Name = "كود الموديل")]
    public string Code { get; set; }

    [Required(ErrorMessage = "اسم الموديل مطلوب")]
    [StringLength(150)]
    [Display(Name = "اسم الموديل")]
    public string Name { get; set; }

    [Display(Name = "الوصف")]
    public string? Description { get; set; }

    // علاقة الربط: كل موديل له قائمة مواد مكونة له
    [ValidateNever]
    public ICollection<BillOfMaterialItem> BillOfMaterialItems { get; set; }
}
