using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Keswa.Models;

public class Customer
{
    public int Id { get; set; }

    [Required(ErrorMessage = "اسم العميل مطلوب.")]
    [Display(Name = "اسم العميل")]
    public string Name { get; set; }

    [Display(Name = "رقم الهاتف")]
    public string PhoneNumber { get; set; }

    [Display(Name = "العنوان")]
    public string Address { get; set; }

    [ValidateNever]
    public ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
}
