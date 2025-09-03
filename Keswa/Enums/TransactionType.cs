using System.ComponentModel.DataAnnotations;

namespace Keswa.Enums;

public enum TransactionType
{
    [Display(Name = "رصيد افتتاحي")]
    OpeningBalance,

    [Display(Name = "تحويل مخزني")]
    InternalTransfer,

    [Display(Name = "استلام من مورد")]
    SupplierReceipt
}
