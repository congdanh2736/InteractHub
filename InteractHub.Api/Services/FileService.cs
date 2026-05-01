using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace InteractHub.Api.Services
{
    public class FileService : IFileService
    {
        private readonly Cloudinary _cloudinary;

        public FileService(IConfiguration config)
        {
            // Lấy thông tin từ appsettings.json
            var account = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadFileAsync(IFormFile file, string containerName = "interacthub-images")
        {
            if (file is null || file.Length == 0)
                throw new ArgumentException("File cannot be empty.");

            // Mở luồng đọc file
            using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = containerName // Tạo folder trên Cloudinary để dễ quản lý
            };

            // Thực hiện upload lên Cloudinary
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
                throw new Exception(uploadResult.Error.Message);

            // Trả về đường link an toàn (https) trực tiếp của bức ảnh
            return uploadResult.SecureUrl.ToString();
        }
    }
}