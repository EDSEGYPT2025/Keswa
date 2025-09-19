using Keswa.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keswa.Models;

public class Material
{
    // هذا يضمن أن قائمة الأصناف لن تكون فارغة أبداً
    public Material()
    {
        InventoryItems = new HashSet<InventoryItem>();
    }

    public int Id { get; set; }

    [Required(ErrorMessage = "اسم المادة مطلوب")]
    [StringLength(100)]
    [Display(Name = "اسم المادة")]
    public string Name { get; set; }

    [Display(Name = "اللون")]
    public int? ColorId { get; set; } // Foreign Key
    [ValidateNever]
    [ForeignKey("ColorId")]
    public Color? Color { get; set; } // Navigation Property

    [Required(ErrorMessage = "وحدة القياس مطلوبة")]
    [Display(Name = "وحدة القياس")]
    public UnitOfMeasure Unit { get; set; }

    // علاقة الربط: كل مادة يمكن أن تكون موجودة في عدة أرصدة مخزنية
    public ICollection<InventoryItem> InventoryItems { get; set; }
}
