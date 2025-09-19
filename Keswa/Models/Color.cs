using System.ComponentModel.DataAnnotations;

namespace Keswa.Models
{
    public class Color
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم اللون مطلوب")]
        [Display(Name = "اسم اللون")]
        public string Name { get; set; }
    }
}
