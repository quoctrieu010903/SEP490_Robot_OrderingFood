

namespace SEP490_Robot_FoodOrdering.Application.Abstractions.Utils
{
    public interface IUtilsService
    {
        /// <summary>
        /// Generates a random numeric string with specified length
        /// </summary>
        /// <param name="length">The length of the token to generate</param>
        /// <returns>Random numeric string</returns>
        string GenerateRandomOtp(int length);

      
        /// <summary>
        /// Hashes a password using BCrypt's EnhancedHashPassword method with SHA-512.
        /// </summary>
        /// <param name="content">The password to hash.</param>
        /// <returns>A hashed version of the password, including the salt.</returns>
        string HashPassword(string content);

        /// <summary>
        /// Verifies a password against a hashed password using SHA-512.
        /// </summary>
        /// <param name="content">The plain-text password to verify.</param>
        /// <param name="hashedContent">The hashed password for comparison.</param>
        /// <returns>True if the password matches the hashed value, otherwise false.</returns>
        bool VerifyPassword(string content, string hashedContent);

        /// <summary>
        /// Retrieves the content of an email template file from the specified folder.
        /// </summary>
        /// <param name="templateName">The name of the template file to retrieve.</param>
        /// <param name="folder">The folder where the template file is located.</param>
        /// <returns>The content of the template file as a string.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the template file does not exist at the specified location.</exception>
        string GetEmailTemplate(string templateName, string folder);
    }
}
