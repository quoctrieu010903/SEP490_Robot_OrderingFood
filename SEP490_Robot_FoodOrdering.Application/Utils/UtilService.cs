
using Microsoft.Extensions.Hosting;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Utils;
using SEP490_Robot_FoodOrdering.Domain.Interface;

namespace SEP490_Robot_FoodOrdering.Application.Utils
{
    public class UtilService : IUtilsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHostEnvironment _env;
        public UtilService(IUnitOfWork unitOfWork , IHostEnvironment env)
        {
            _unitOfWork = unitOfWork;
            _env = env;
        }


        public string GenerateRandomOtp(int length)
        {
            return new string(Enumerable.Repeat("0123456789", length)
                .Select(s => s[Random.Shared.Next(s.Length)])
                .ToArray());
        }

        public async Task<string> GetEmailTemplateAsync(string templateName, string folder)
        {
            var contentRoot = _env.ContentRootPath; // root của project (nơi chứa Program.cs)
            var wwwrootPath = Path.Combine(contentRoot, "wwwroot");
            var filePath = Path.Combine(wwwrootPath, folder, templateName);
           

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Template not found: {filePath}");

            return await File.ReadAllTextAsync(filePath);
        }

        public string HashPassword(string content)
        {
            return BCrypt.Net.BCrypt.EnhancedHashPassword(content, BCrypt.Net.HashType.SHA512);
        }

        public bool VerifyPassword(string content, string hashedContent)
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(content, hashedContent, BCrypt.Net.HashType.SHA512);
        }

        //public string GetEmailTemplate(string templateName, string folder)
        //{
        //    var templatePath = Path.Combine(_environment.WebRootPath, $"{folder}/{templateName}");

        //    if (File.Exists(templatePath))
        //    {
        //        return File.ReadAllText(templatePath);
        //    }
        //    else
        //    {
        //        throw new FileNotFoundException($"Template file '{templateName}' not found in '{folder}'");
        //    }
        //}
    }
}
