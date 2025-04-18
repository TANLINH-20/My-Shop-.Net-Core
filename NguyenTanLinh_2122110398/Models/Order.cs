using System.Text.Json.Serialization;

namespace NguyenTanLinh_2122110398.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate
        {
            get; set;
        }
        public decimal Total { get; set; }
        public required string Status { get; set; }
        public required string ShippingAddress { get; set; }
        public required string PaymentMethod { get; set; }
        public User? User { get; set; }
        public List<OrderDetail>? OrderDetails { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}