namespace NguyenTanLinh_2122110398.Dtos
{
    public class OrderDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal Total { get; set; }
        public required string Status { get; set; }
        public required string ShippingAddress { get; set; }
        public required string PaymentMethod { get; set; }
        public List<OrderDetailDto>? OrderDetails { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}