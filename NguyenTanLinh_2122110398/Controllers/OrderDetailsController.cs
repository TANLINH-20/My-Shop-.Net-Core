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
    public class OrderDetailsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrderDetailsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<OrderDetailDto>>> GetOrderDetails()
        {
            var orderDetails = await _context.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.Product)
                .Select(od => new OrderDetailDto
                {
                    Id = od.Id,
                    OrderId = od.OrderId,
                    ProductId = od.ProductId,
                    Quantity = od.Quantity,
                    Price = od.Price,
                    SubTotal = od.SubTotal,
                    ProductName = od.Product != null ? od.Product.Name : ""
                })
                .ToListAsync();
            return Ok(orderDetails);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<OrderDetailDto>> GetOrderDetail(int id)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            var orderDetail = await _context.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.Product)
                .FirstOrDefaultAsync(od => od.Id == id);

            if (orderDetail == null) return NotFound("Chi tiết đơn hàng không tồn tại.");

            if (userRole != "Admin" && orderDetail.Order?.UserId != userId)
                return Unauthorized("Bạn không có quyền xem chi tiết đơn hàng này.");

            var orderDetailDto = new OrderDetailDto
            {
                Id = orderDetail.Id,
                OrderId = orderDetail.OrderId,
                ProductId = orderDetail.ProductId,
                Quantity = orderDetail.Quantity,
                Price = orderDetail.Price,
                SubTotal = orderDetail.SubTotal,
                ProductName = orderDetail.Product != null ? orderDetail.Product.Name : ""
            };

            return Ok(orderDetailDto);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<OrderDetailDto>> PostOrderDetail(OrderDetailDto orderDetailDto)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            var order = await _context.Orders.FindAsync(orderDetailDto.OrderId);
            if (order == null) return NotFound("Đơn hàng không tồn tại.");
            if (order.UserId != userId && !User.IsInRole("Admin"))
                return Unauthorized("Bạn không có quyền thêm chi tiết cho đơn hàng này.");

            var orderDetail = new OrderDetail
            {
                OrderId = orderDetailDto.OrderId,
                ProductId = orderDetailDto.ProductId,
                Quantity = orderDetailDto.Quantity,
                Price = orderDetailDto.Price,
                SubTotal = orderDetailDto.SubTotal
            };

            _context.OrderDetails.Add(orderDetail);
            await _context.SaveChangesAsync();

            orderDetailDto.Id = orderDetail.Id;
            orderDetailDto.ProductName = (await _context.Products.FindAsync(orderDetail.ProductId))?.Name ?? "";
            return CreatedAtAction(nameof(GetOrderDetail), new { id = orderDetail.Id }, orderDetailDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutOrderDetail(int id, OrderDetailDto orderDetailDto)
        {
            if (id != orderDetailDto.Id) return BadRequest("ID không khớp.");

            var orderDetail = await _context.OrderDetails.FindAsync(id);
            if (orderDetail == null) return NotFound("Chi tiết đơn hàng không tồn tại.");

            orderDetail.OrderId = orderDetailDto.OrderId;
            orderDetail.ProductId = orderDetailDto.ProductId;
            orderDetail.Quantity = orderDetailDto.Quantity;
            orderDetail.Price = orderDetailDto.Price;
            orderDetail.SubTotal = orderDetailDto.SubTotal;

            _context.Entry(orderDetail).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteOrderDetail(int id)
        {
            var orderDetail = await _context.OrderDetails.FindAsync(id);
            if (orderDetail == null) return NotFound("Chi tiết đơn hàng không tồn tại.");

            _context.OrderDetails.Remove(orderDetail);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("order/{orderId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<OrderDetailDto>>> GetOrderDetailByOrderId(int orderId)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound("Đơn hàng không tồn tại.");
            if (userRole != "Admin" && order.UserId != userId)
                return Unauthorized("Bạn không có quyền xem chi tiết đơn hàng này.");

            var orderDetails = await _context.OrderDetails
                .Where(od => od.OrderId == orderId)
                .Include(od => od.Product)
                .Select(od => new OrderDetailDto
                {
                    Id = od.Id,
                    OrderId = od.OrderId,
                    ProductId = od.ProductId,
                    Quantity = od.Quantity,
                    Price = od.Price,
                    SubTotal = od.SubTotal,
                    ProductName = od.Product != null ? od.Product.Name : ""
                })
                .ToListAsync();

            return Ok(orderDetails);
        }
    }
}