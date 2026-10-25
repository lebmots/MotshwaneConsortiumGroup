using System.ComponentModel.DataAnnotations;
namespace MotshwaneConsortiumGroup.DTOs
{
    public class CreateUnitDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = "";

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = "";

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = "";

        [Range(0, double.MaxValue)]
        public decimal PriceFrom { get; set; }

        public bool Available { get; set; } = true;
    }
}
