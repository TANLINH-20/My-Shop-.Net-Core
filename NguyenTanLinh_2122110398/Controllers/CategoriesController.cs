using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NguyenTanLinh_2122110398.Data;
using NguyenTanLinh_2122110398.Dtos;
using NguyenTanLinh_2122110398.Models;
using System.Security.Claims;

namespace NguyenTanLinh_2122110398.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var categories = await _context.Categories
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    CreatedDate = c.CreatedDate,
                    CreatedBy = c.CreatedBy,
                    UpdatedDate = c.UpdatedDate,
                    UpdatedBy = c.UpdatedBy
                })
                .ToListAsync();
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound("Danh mục không tồn tại.");

            var categoryDto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedDate = category.CreatedDate,
                CreatedBy = category.CreatedBy,
                UpdatedDate = category.UpdatedDate,
                UpdatedBy = category.UpdatedBy,
                Products = category.Products?.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Description = p.Description,
                    Image = p.Image,
                    Stock = p.Stock,
                    CategoryId = p.CategoryId,
                    CreatedDate = p.CreatedDate,
                    CreatedBy = p.CreatedBy,
                    UpdatedDate = p.UpdatedDate,
                    UpdatedBy = p.UpdatedBy
                }).ToList()
            };

            return Ok(categoryDto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CategoryDto>> PostCategory(CategoryDto categoryDto)
        {
            var category = new Category
            {
                Name = categoryDto.Name,
                Description = categoryDto.Description,
                IsActive = categoryDto.IsActive,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = User.FindFirst(ClaimTypes.Name)?.Value ?? "system"
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            categoryDto.Id = category.Id;
            categoryDto.CreatedDate = category.CreatedDate;
            categoryDto.CreatedBy = category.CreatedBy;
            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, categoryDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutCategory(int id, CategoryDto categoryDto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound("Danh mục không tồn tại.");

            category.Name = categoryDto.Name;
            category.Description = categoryDto.Description;
            category.IsActive = categoryDto.IsActive;
            category.UpdatedDate = DateTime.UtcNow;
            category.UpdatedBy = User.FindFirst(ClaimTypes.Name)?.Value ?? "system";

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound("Danh mục không tồn tại.");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}