using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bulky_Razor_Pages_.Model
{
    public class Categories
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Category Name")]
        [MaxLength(20)]
        public string Name { get; set; } = new string(string.Empty);

        [DisplayName("Display Order")]
        [Range(0, 100, ErrorMessage = "The number must be between 0 and 100")]
        public int DisplayOrder { get; set; }
    }
}
