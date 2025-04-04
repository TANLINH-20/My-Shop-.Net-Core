using System.Text.Json.Serialization;

namespace NguyenTanLinh_2122110398.Model
{
    public class Product
    {
        public int Id { get; set; }  // Khóa chính
        public string Name { get; set; }  // Tên sản phẩm
        public float Price { get; set; }  // Giá sản phẩm
        public string Description { get; set; }  // Mô tả sản phẩm
        [JsonIgnore]
        public Category Category { get; set; }
    }
}
