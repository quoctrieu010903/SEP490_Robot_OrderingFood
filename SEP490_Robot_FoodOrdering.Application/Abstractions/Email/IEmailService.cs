using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_Robot_FoodOrdering.Application.Abstractions.Email
{
    public interface IEmailService
    {
        /// <summary>
        /// Sends an email with a verification code to the specified email address
        /// </summary>
        /// <param name="to">The recipient's email address</param>
        /// <param name="token">The verification code to be sent</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task SendVerificationEmailAsync(string to, string token);

        /// <summary>
        /// Sends an email with a password reset code to the specified email address
        /// </summary>
        /// <param name="to">The recipient's email address</param>
        /// <param name="token">The password reset code to be sent</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task SendPasswordResetEmailAsync(string to, string token);
    }
}
