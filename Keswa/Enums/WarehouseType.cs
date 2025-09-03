using System.ComponentModel.DataAnnotations;

namespace Keswa.Enums
{
    public enum WarehouseType
    {
        [Display(Name = "مواد خام")]
        RawMaterials,

        [Display(Name = "منتجات نهائية")]
        FinishedGoods
    }
}
