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
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly FileUploadService _fileUploadService;

        public UsersController(AppDbContext context, FileUploadService fileUploadService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Role = u.Role,
                    Email = u.Email,
                    FullName = u.FullName,
                    Image = u.Image,
                    Address = u.Address,
                    CreatedDate = u.CreatedDate,
                    CreatedBy = u.CreatedBy,
                    UpdatedDate = u.UpdatedDate,
                    UpdatedBy = u.UpdatedBy
                })
                .ToListAsync();
            return Ok(users);
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> PostUser(UserRegisterDto registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
                return BadRequest("Email đã tồn tại.");

            var user = new User
            {
                Email = registerDto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                FullName = registerDto.FullName,
                Address = registerDto.Address,
                Role = registerDto.Role ?? "Customer",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = User.FindFirst(ClaimTypes.Name)?.Value ?? "system"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userDto = new UserDto
            {
                Id = user.Id,
                Role = user.Role,
                Email = user.Email,
                FullName = user.FullName,
                Image = user.Image,
                Address = user.Address,
                CreatedDate = user.CreatedDate,
                CreatedBy = user.CreatedBy,
                UpdatedDate = user.UpdatedDate,
                UpdatedBy = user.UpdatedBy
            };

            return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, userDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutUser(int id, [FromForm] UserDto userDto, IFormFile? imageFile)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Người dùng không tồn tại.");
            // Xử lý ảnh
            string? imagePath = user.Image; // Giữ ảnh cũ nếu không upload ảnh mới
            if (!string.IsNullOrEmpty(userDto.Image) && userDto.Image != user.Image)
            {
                imagePath = userDto.Image; // Sử dụng đường dẫn mới nếu gửi từ client
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

            user.Role = userDto.Role ?? "Customer";
            user.Email = userDto.Email;
            user.FullName = userDto.FullName;
            user.Address = userDto.Address;
            user.Image = userDto.Image;
            user.UpdatedDate = DateTime.UtcNow;
            user.UpdatedBy = User.Identity?.Name ?? "system";

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Người dùng không tồn tại.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("address")]
        [Authorize]
        public async Task<IActionResult> UpdateAddress([FromBody] AddressDto addressDto)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized("Không xác định được người dùng.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("Người dùng không tồn tại.");

            user.Address = addressDto.Address;
            user.UpdatedDate = DateTime.UtcNow;
            user.UpdatedBy = User.FindFirst(ClaimTypes.Name)?.Value ?? "system";

            await _context.SaveChangesAsync();

            var userDto = new UserDto
            {
                Id = user.Id,
                Role = user.Role,
                Email = user.Email,
                FullName = user.FullName,
                Image = user.Image,
                Address = user.Address,
                CreatedDate = user.CreatedDate,
                CreatedBy = user.CreatedBy,
                UpdatedDate = user.UpdatedDate,
                UpdatedBy = user.UpdatedBy
            };

            return Ok(userDto);
        }

        [HttpPut("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized("Không xác định được người dùng.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("Người dùng không tồn tại.");

            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.Password))
                return BadRequest("Mật khẩu hiện tại không đúng.");

            user.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
            user.UpdatedDate = DateTime.UtcNow;
            user.UpdatedBy = User.FindFirst(ClaimTypes.Name)?.Value ?? "system";

            await _context.SaveChangesAsync();
            return NoContent();
        }
        // GET: api/users/profile
        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetProfile()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized("Không xác định được người dùng.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFound("Người dùng không tồn tại.");

            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Address = user.Address,
                Role = user.Role,
                CreatedDate = user.CreatedDate,
                CreatedBy = user.CreatedBy,
                UpdatedDate = user.UpdatedDate,
                UpdatedBy = user.UpdatedBy
            };

            return Ok(userDto);
        }
    }
}