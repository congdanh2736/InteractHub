using Microsoft.AspNetCore.Http;

namespace InteractHub.Api.Services
{
    public interface IFileService
    {
        // nhận 1 file từ fe và trả về url của file đó trên clound
        Task<string> UploadFileAsync(IFormFile file, string containerName = "interacthub-images");
    }
}
