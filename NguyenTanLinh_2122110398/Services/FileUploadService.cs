using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NguyenTanLinh_2122110398.Services
{
    public class FileUploadService
    {
        private readonly IConfiguration _configuration;

        public FileUploadService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ hoặc rỗng.");

            var uploadDir = _configuration["UploadDir"];
            if (string.IsNullOrEmpty(uploadDir))
                throw new InvalidOperationException("Upload directory is not configured in appsettings.json (UploadDir).");

            // Tạo đường dẫn đầy đủ
            var fullUploadPath = Path.Combine(Directory.GetCurrentDirectory(), uploadDir);

            // Tạo thư mục nếu chưa tồn tại
            if (!Directory.Exists(fullUploadPath))
            {
                Directory.CreateDirectory(fullUploadPath);
            }

            // Tạo tên file duy nhất
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(fullUploadPath, fileName);

            // Lưu file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Trả về đường dẫn tương đối
            return Path.Combine(uploadDir, fileName).Replace("\\", "/");
        }
    }
}