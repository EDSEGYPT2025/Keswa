using System.ComponentModel.DataAnnotations;

namespace Keswa.Enums;

public enum PurchaseOrderStatus
{
    [Display(Name = "جديد")]
    New,

    [Display(Name = "تم الطلب")]
    Ordered,

    [Display(Name = "تم الاستلام")]
    Received
}
