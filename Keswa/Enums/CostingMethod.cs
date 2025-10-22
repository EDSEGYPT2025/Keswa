using System.ComponentModel.DataAnnotations;

namespace Keswa.Enums
{
    public enum CostingMethod
    {
        [Display(Name = "متوسط السعر")]
        Average,

        [Display(Name = "أعلى سعر شراء")]
        Highest,

        [Display(Name = "آخر سعر شراء")]
        Last
    }
}