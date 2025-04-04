using System.Text.Json.Serialization;

namespace NguyenTanLinh_2122110398.Model
{
    public class Category
    {
        public int Id { get; set; }  // Khóa chính
        public string Name { get; set; }  // Tên thể loại
        public string Description { get; set; }  // Mô tả thể loại

        // Liên kết một thể loại có nhiều sản phẩm
        [JsonIgnore]
        public ICollection<Product>? Products { get; set; }
    }
}
