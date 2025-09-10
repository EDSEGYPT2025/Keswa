using System.ComponentModel.DataAnnotations;

namespace Keswa.Enums
{
    public enum ProductionLine
    {
        [Display(Name = "خط إنتاج العباية")]
        Abaya,

        [Display(Name = "خط إنتاج النقاب")]
        Niqab
    }
}