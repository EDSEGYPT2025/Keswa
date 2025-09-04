using System.ComponentModel.DataAnnotations;

namespace Keswa.Enums
{
    public enum BalanceType
    {
        [Display(Name = "مدين (عليه)")]
        Debit,

        [Display(Name = "دائن (له)")]
        Credit
    }
}
