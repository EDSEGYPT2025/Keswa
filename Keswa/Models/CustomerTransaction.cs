using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keswa.Models
{
    public class CustomerTransaction
    {
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime TransactionDate { get; set; }

        [Required]
        public string TransactionType { get; set; } // e.g., "Opening Balance", "Sales Invoice", "Receipt"

        public string? Reference { get; set; } // e.g., Invoice Number

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Debit { get; set; } // مدين (عليه)

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Credit { get; set; } // دائن (له)
    }
}
