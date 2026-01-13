
using Microsoft.Extensions.Hosting;
using QRCoder;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Utils;
using SEP490_Robot_FoodOrdering.Core.Ultils;
using SEP490_Robot_FoodOrdering.Domain.Enums;
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

        public string GenerateEmploymentCode(RoleNameEnums role, int length = 4)
        {
            string prefix = role switch
            {
                RoleNameEnums.Admin => "AD",
                RoleNameEnums.Chef => "CH",
                RoleNameEnums.Waiter => "WA",
                RoleNameEnums.Moderator => "MO",
                _ => throw new ArgumentException("Invalid role", nameof(role))
            };
            string guidPart = Guid.NewGuid().ToString("N").ToUpper().Substring(0, length);
            return $"{prefix}-{guidPart}";
        }

        public string GenerateRandomOtp(int length)
        {
            return new string(Enumerable.Repeat("0123456789", length)
                .Select(s => s[Random.Shared.Next(s.Length)])
                .ToArray());
        }
        public string GenerateRandomString(int length, CharacterSet charSet)
        {
            if (length <= 0)
            {
                throw new ArgumentException("Length must be greater than 0", nameof(length));
            }

            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string numbers = "0123456789";

            string characters = charSet switch
            {
                CharacterSet.Uppercase => uppercase,
                CharacterSet.Lowercase => lowercase,
                CharacterSet.Mixed => lowercase + numbers,
                CharacterSet.AlphanumericMixed => uppercase + lowercase + numbers,
                _ => throw new ArgumentException("Invalid character set", nameof(charSet))
            };

            return new string(Enumerable.Range(0, length)
                .Select(_ => characters[Random.Shared.Next(characters.Length)])
                .ToArray());
            ;
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
            return BCrypt.Net.BCrypt.Verify(content, hashedContent);
        }
        public string GenerateQrCodeBase64_NoDrawing(string text)
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeImage = qrCode.GetGraphic(20);
            return Convert.ToBase64String(qrCodeImage);
        }

        public string GenerateCode(string prefix, int length)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                throw new ArgumentException("prefix là bắt buộc.", nameof(prefix));

            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length), "length phải > 0.");

            var p = prefix.Trim().ToUpperInvariant();

            // Nếu enum của bạn là AlphaNumericUpper thì dùng cái đó.
            // Nếu không có, dùng AlphaNumeric rồi .ToUpperInvariant()
            var random = GenerateRandomString(length, CharacterSet.AlphanumericMixed).ToUpperInvariant();

            return $"{p}-{random}";
        }

       
                /// <summary>
        /// Parse resolutionNote để extract các item name
        /// Ví dụ: "Phục vụ nhanh: Cho thêm nước mắm, cho thêm nước tương"
        /// → ["Nước mắm", "Nước tương"]
        /// </summary>
        public List<string> ParseQuickServeItems(string resolutionNote)
        {
            var items = new List<string>();

            if (string.IsNullOrWhiteSpace(resolutionNote))
                return items;

            // Loại bỏ prefix "Phục vụ nhanh:" hoặc "Yêu cầu nhanh:" nếu có
            var cleanedNote = resolutionNote;
            var prefixes = new[] { "Phục vụ nhanh:", "Yêu cầu nhanh:", "Phục vụ nhanh", "Yêu cầu nhanh" };
            foreach (var prefix in prefixes)
            {
                if (cleanedNote.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    cleanedNote = cleanedNote.Substring(prefix.Length).Trim();
                    break;
                }
            }

            // Tách các item bằng dấu phẩy
            var parts = cleanedNote.Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                
                // Loại bỏ các prefix như "Cho thêm", "Thêm", "Cho" nếu có
                var prefixesToRemove = new[] { "Cho thêm", "Thêm", "Cho", "cho thêm", "thêm", "cho" };
                foreach (var prefix in prefixesToRemove)
                {
                    if (trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        trimmed = trimmed.Substring(prefix.Length).Trim();
                        break;
                    }
                }

                // Chỉ thêm nếu không rỗng
                if (!string.IsNullOrWhiteSpace(trimmed))
                {
                    // Viết hoa chữ cái đầu, giữ nguyên phần còn lại
                    var normalized =
                        char.ToUpper(trimmed[0]) + (trimmed.Length > 1 ? trimmed.Substring(1) : string.Empty);
                    items.Add(normalized);
                }
            }

            return items;
        }


    }
}
