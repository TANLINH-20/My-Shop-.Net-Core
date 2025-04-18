using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NguyenTanLinh_2122110398.Data;
using NguyenTanLinh_2122110398.Dtos;
using NguyenTanLinh_2122110398.Models;
using NguyenTanLinh_2122110398.Services;
using System.Security.Claims;

namespace NguyenTanLinh_2122110398.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly FileUploadService _fileUploadService;

        public ProductsController(AppDbContext context, FileUploadService fileUploadService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    CategoryId = p.CategoryId,
                    Description = p.Description,
                    Image = p.Image,
                    Stock = p.Stock,
                    CreatedDate = p.CreatedDate,
                    CreatedBy = p.CreatedBy,
                    UpdatedDate = p.UpdatedDate,
                    UpdatedBy = p.UpdatedBy,
                    CategoryName = p.Category != null ? p.Category.Name : ""
                })
                .ToListAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound("Sản phẩm không tồn tại.");

            var productDto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                CategoryId = product.CategoryId,
                Description = product.Description,
                Image = product.Image,
                Stock = product.Stock,
                CreatedDate = product.CreatedDate,
                CreatedBy = product.CreatedBy,
                UpdatedDate = product.UpdatedDate,
                UpdatedBy = product.UpdatedBy,
                CategoryName = product.Category != null ? product.Category.Name : ""
            };
            return Ok(productDto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductDto>> PostProduct([FromForm] ProductDto productDto, IFormFile? imageFile)
        {
            string? imagePath = null;
            if (!string.IsNullOrEmpty(productDto.Image))
            {
                imagePath = productDto.Image;
            }
            else if (imageFile != null && imageFile.Length > 0)
            {
                try
                {
                    imagePath = await _fileUploadService.UploadFileAsync(imageFile);
                }
                catch (Exception ex)
                {
                    return BadRequest($"Lỗi khi upload file: {ex.Message}");
                }
            }

            var product = new Product
            {
                Name = productDto.Name,
                Price = productDto.Price,
                CategoryId = productDto.CategoryId,
                Description = productDto.Description,
                Image = imagePath ?? productDto.Image,
                Stock = productDto.Stock,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = User.FindFirst(ClaimTypes.Name)?.Value ?? "system"
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            productDto.Id = product.Id;
            productDto.Image = product.Image;
            productDto.CreatedDate = product.CreatedDate;
            productDto.CreatedBy = product.CreatedBy;
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, productDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutProduct(int id, [FromForm] ProductDto productDto, IFormFile? imageFile)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound("Sản phẩm không tồn tại.");

            string? imagePath = null;
            if (!string.IsNullOrEmpty(productDto.Image))
            {
                imagePath = productDto.Image;
            }
            else if (imageFile != null && imageFile.Length > 0)
            {
                try
                {
                    imagePath = await _fileUploadService.UploadFileAsync(imageFile);
                }
                catch (Exception ex)
                {
                    return BadRequest($"Lỗi khi upload file: {ex.Message}");
                }
            }

            product.Name = productDto.Name;
            product.Price = productDto.Price;
            product.CategoryId = productDto.CategoryId;
            product.Description = productDto.Description;
            product.Image = imagePath ?? productDto.Image;
            product.Stock = productDto.Stock;
            product.UpdatedDate = DateTime.UtcNow;
            product.UpdatedBy = User.FindFirst(ClaimTypes.Name)?.Value ?? "system";

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound("Sản phẩm không tồn tại.");

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}