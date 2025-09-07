
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Cloudinary;
using SEP490_Robot_FoodOrdering.Infrastructure.DependencyInjection.Options;


namespace SEP490_Robot_FoodOrdering.Infrastructure.Cloudinary
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly CloudinaryOptions _cloudinaryOptions;
        private CloudinaryDotNet.Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinaryOptions> cloudinaryOptions)
        {
            _cloudinaryOptions = cloudinaryOptions.Value;
            InitialCloudinary();
        }

        private void InitialCloudinary()
        {
            var account = new Account(
                _cloudinaryOptions.Name,
                _cloudinaryOptions.ApiKey,
                _cloudinaryOptions.ApiSecret
            );
            _cloudinary = new CloudinaryDotNet.Cloudinary(account);
        }


        public async Task<string> UploadImageAsync(IFormFile file, string? folder, string? oldFile)
        {
            if (oldFile is not null && oldFile.StartsWith(_cloudinaryOptions.Url))
            {
                await DeleteImageAsync(oldFile);
            }

            var stream = file.OpenReadStream();
            var fileExtension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid():N}{fileExtension}";
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(uniqueFileName, stream),
                UseFilename = true,
                Folder = folder ?? "/",
                UniqueFilename = true,
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            return uploadResult.SecureUrl.OriginalString;

        }
        public async Task DeleteImageAsync(string oldFile)
        {
            string baseUrl = _cloudinaryOptions.Url;
            string publicId = oldFile.Replace(baseUrl, "").Split('.')[0];
            var deletionParams = new DeletionParams(publicId) { ResourceType = CloudinaryDotNet.Actions.ResourceType.Image };
            await _cloudinary.DestroyAsync(deletionParams);
        }

        public async Task<string> UploadFileAsync(IFormFile file, string? folder, string? oldFile, CloudinaryDotNet.Actions.ResourceType resourceType)
        {
            if (!string.IsNullOrWhiteSpace(oldFile) && oldFile.StartsWith(_cloudinaryOptions.Url))
            {
                await DeleteFileAsync(oldFile, resourceType);
            }

            var stream = file.OpenReadStream();
            var fileExtension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid():N}{fileExtension}";

            RawUploadResult? uploadResult = null;

            switch (resourceType)
            {
                case ResourceType.Image:
                    var imageUploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(uniqueFileName, stream),
                        Folder = folder ?? "/",
                        UseFilename = true,
                        UniqueFilename = true
                    };
                    var imageResult = await _cloudinary.UploadAsync(imageUploadParams);
                    uploadResult = imageResult;
                    break;

                case ResourceType.Video:
                    var videoUploadParams = new VideoUploadParams
                    {
                        File = new FileDescription(uniqueFileName, stream),
                        Folder = folder ?? "/",
                        UseFilename = true,
                        UniqueFilename = true
                    };
                    var videoResult = await _cloudinary.UploadAsync(videoUploadParams);
                    uploadResult = videoResult;
                    break;

                default: // Mặc định coi là file Raw (PDF, DOCX, ZIP,...)
                    var rawUploadParams = new RawUploadParams
                    {
                        File = new FileDescription(uniqueFileName, stream),
                        Folder = folder ?? "/",
                        UseFilename = true,
                        UniqueFilename = true
                    };
                    var rawResult = await _cloudinary.UploadAsync(rawUploadParams, "auto");
                    uploadResult = rawResult;
                    break;
            }

            return uploadResult.SecureUrl?.OriginalString ?? string.Empty;
        }

        public async Task DeleteFileAsync(string oldFile, CloudinaryDotNet.Actions.ResourceType resourceType)
        {
            string baseUrl = _cloudinaryOptions.Url;
            string publicId = oldFile.Replace(baseUrl, "").Split('.')[0];

            var deletionParams = new DeletionParams(publicId)
            {
                ResourceType = resourceType
            };

            await _cloudinary.DestroyAsync(deletionParams);
        }

    }
}
