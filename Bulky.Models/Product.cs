using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Bulky.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; } = new string(string.Empty);
        public string Description { get; set; } = new string(string.Empty);
        [Required]
        public string ISBN { get; set; } = new string(string.Empty);
        [Required]
        public string Author { get; set; } = new string(string.Empty);
        [Required]
        [DisplayName("List Price")]
        [Range(0,1000)]
        public double ListPrice { get; set; }
        [Required]
        [DisplayName("Price for (1-50)")]
        [Range(0, 1000)]
        public double Price { get; set; }
        [Required]
        [DisplayName("Price for 50+")]
        [Range(0, 1000)]
        public double Price50 { get; set; }
        [Required]
        [DisplayName("Price for 100+")]
        [Range(0, 1000)]
        public double Price100 { get; set; }

        public List<ProductImage>? ProductImages { get; set; }
        [ValidateNever]
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category? Category { get; set; }

    }
}
