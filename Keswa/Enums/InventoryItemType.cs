using System.ComponentModel.DataAnnotations;

namespace Keswa.Enums
{
    public enum InventoryItemType
    {
        [Display(Name = "مادة خام")]
        RawMaterial,

        [Display(Name = "منتج نهائي")]
        FinishedGood
    }
}
