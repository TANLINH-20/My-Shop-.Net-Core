using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NguyenTanLinh_2122110398.Data;
using NguyenTanLinh_2122110398.Dtos;
using NguyenTanLinh_2122110398.Models;

namespace NguyenTanLinh_2122110398.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized("Không xác định được người dùng.");

            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            var query = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .AsQueryable();

            if (userRole != "Admin")
            {
                query = query.Where(o => o.UserId == userId);
            }

            var orders = await query
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    OrderDate = o.OrderDate,
                    Total = o.Total,
                    Status = o.Status,
                    ShippingAddress = o.ShippingAddress,
                    PaymentMethod = o.PaymentMethod,
                    OrderDetails = o.OrderDetails.Select(od => new OrderDetailDto
                    {
                        Id = od.Id,
                        ProductId = od.ProductId,
                        Quantity = od.Quantity,
                        Price = od.Price,
                        SubTotal = od.SubTotal,
                        ProductName = od.Product != null ? od.Product.Name : ""
                    }).ToList(),
                    CreatedDate = o.CreatedDate,
                    UpdatedDate = o.UpdatedDate
                })
                .ToListAsync();

            return Ok(orders);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized("Không xác định được người dùng.");

            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound("Đơn hàng không tồn tại.");

            if (userRole != "Admin" && order.UserId != userId)
                return Unauthorized("Bạn không có quyền xem đơn hàng này.");

            var orderDto = new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                Total = order.Total,
                Status = order.Status,
                ShippingAddress = order.ShippingAddress,
                PaymentMethod = order.PaymentMethod,
                OrderDetails = order.OrderDetails.Select(od => new OrderDetailDto
                {
                    Id = od.Id,
                    ProductId = od.ProductId,
                    Quantity = od.Quantity,
                    Price = od.Price,
                    SubTotal = od.SubTotal,
                    ProductName = od.Product != null ? od.Product.Name : ""
                }).ToList(),
                CreatedDate = order.CreatedDate,
                UpdatedDate = order.UpdatedDate
            };

            return Ok(orderDto);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<OrderDto>> PostOrder(OrderDto orderDto)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized("Không xác định được người dùng.");

            var order = new Order
            {
                UserId = userId,
                OrderDate = orderDto.OrderDate,
                Total = orderDto.Total,
                Status = orderDto.Status ?? "Pending",
                ShippingAddress = orderDto.ShippingAddress,
                PaymentMethod = orderDto.PaymentMethod,
                CreatedDate = DateTime.UtcNow,
                OrderDetails = orderDto.OrderDetails?.Select(od => new OrderDetail
                {
                    ProductId = od.ProductId,
                    Quantity = od.Quantity,
                    Price = od.Price,
                    SubTotal = od.SubTotal
                }).ToList() ?? new List<OrderDetail>()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            orderDto.Id = order.Id;
            orderDto.CreatedDate = order.CreatedDate;
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, orderDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutOrder(int id, OrderDto orderDto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound("Đơn hàng không tồn tại.");

            order.OrderDate = orderDto.OrderDate;
            order.Total = orderDto.Total;
            order.Status = orderDto.Status;
            order.ShippingAddress = orderDto.ShippingAddress;
            order.PaymentMethod = orderDto.PaymentMethod;
            order.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound("Đơn hàng không tồn tại.");

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}