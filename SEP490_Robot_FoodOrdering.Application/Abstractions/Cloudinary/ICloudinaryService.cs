using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SEP490_Robot_FoodOrdering.Application.Abstractions.Cloudinary
{
    public interface ICloudinaryService
    {
        /// <summary>
        /// Uploads an image to Cloudinary, optionally deleting an old image first.
        /// </summary>
        /// <param name="file">The image file to upload.</param>
        /// <param name="folder">The folder in Cloudinary to upload the image to. If null, the root folder is used.</param>
        /// <param name="oldFile">The public ID of the old image to delete, if any. If not null, the old image will be removed before uploading the new one.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>Returns the public ID of the uploaded image.</returns>
        Task<string> UploadImageAsync(IFormFile file, string? folder, string? oldFile);

        /// <summary>
        /// Deletes an image from Cloudinary based on the provided public ID.
        /// </summary>
        /// <param name="oldFile">The public ID of the image to delete.</param>
        /// <returns>Asynchronously deletes the image from Cloudinary.</returns>
        Task DeleteImageAsync(string oldFile);

        Task<string> UploadFileAsync(IFormFile file, string? folder, string? oldFile, CloudinaryDotNet.Actions.ResourceType resourceType);

        Task DeleteFileAsync(string oldFile, CloudinaryDotNet.Actions.ResourceType resourceType);
    }
}
