using System.Text.Json.Serialization;

namespace NguyenTanLinh_2122110398.Models
{
    public class Product
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public int Stock { get; set; }
        public int CategoryId { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public Category? Category { get; set; }
    }
}