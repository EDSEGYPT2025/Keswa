using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Keswa.Models
{
    public class SalesOrder
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "يجب اختيار العميل.")]
        [Display(Name = "العميل")]
        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        [ValidateNever]
        public Customer Customer { get; set; }

        [Required(ErrorMessage = "يجب إدخال تاريخ الطلب.")]
        [Display(Name = "تاريخ الطلب")]
        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; }

        [Required(ErrorMessage = "يجب إدخال تاريخ التسليم المتوقع.")]
        [Display(Name = "تاريخ التسليم المتوقع")]
        [DataType(DataType.Date)]
        public DateTime ExpectedDeliveryDate { get; set; }

        [Display(Name = "حالة الطلبية")]
        public Enums.SalesOrderStatus Status { get; set; }

        // *** تم التعديل هنا ***
        public List<SalesOrderDetail> Details { get; set; } = new List<SalesOrderDetail>();
    }
}
