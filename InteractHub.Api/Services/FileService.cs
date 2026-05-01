//using Azure.Storage.Blobs;
//using Azure.Storage.Blobs.Models;

namespace InteractHub.Api.Services
{
    public class FileService : IFileService
    {
        //private readonly BlobServiceClient _blobServiceClient;
        private readonly IWebHostEnvironment _env;

        public FileService(IWebHostEnvironment env)
        {
            // lay connectionString
            //var connectionString = configuration.GetConnectionString("AzureBlobStorage");
            //_blobServiceClient = new BlobServiceClient(connectionString);

            _env = env;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string containerName = "interacthub-images")
        {
            if (file is null || file.Length == 0)
                throw new ArgumentException("File cannot be empty.");


            string webRootPath = _env.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRootPath))
                webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            string uploadsFolder = Path.Combine(webRootPath, containerName);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            //var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName); // tim/tao thu muc tren cloud

            //await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var fileExtension = Path.GetExtension(file.FileName); // doi ten file de khong bi trung
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            //var blobClient = blobContainerClient.GetBlobClient(uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))  
            {
                //await blobClient.UploadAsync(stream, new BlobUploadOptions
                //{
                //    HttpHeaders = new BlobHttpHeaders
                //    {
                //        ContentType = file.ContentType
                //    }
                //});

                await file.CopyToAsync(stream);
            }

            string fileUrl = $"/{containerName}/{uniqueFileName}";

            //return blobClient.Uri.ToString(); // tra ve duong link truc tiep cua tam anh

            return fileUrl;
        }
    }
}
