﻿using System.Text.Json.Serialization;

namespace NguyenTanLinh_2122110398.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal SubTotal { get; set; }
        public Order? Order { get; set; }
        public Product? Product { get; set; }
    }
}