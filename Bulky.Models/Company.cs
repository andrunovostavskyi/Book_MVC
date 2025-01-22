using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Models
{
    public class Company
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Name { get; set; }

        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }

        [MaxLength(50)]
        [Required(ErrorMessage = "The Phone Number is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.(+1(123)4567890)")]
        public string? PhoneNumber { get; set; }
    }
}
