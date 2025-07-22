using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_Robot_FoodOrdering.Core.Constants
{
    public class ResponseCodeConstants
    {
        public const string NOT_FOUND = "Not found!";
        public const string SUCCESS = "Success!";
        public const string FAILED = "Failed!";
        public const string EXISTED = "Existed!";
        public const string DUPLICATE = "Duplicate!";
        public const string INTERNAL_SERVER_ERROR = "Internal server error!";
        public const string INVALID_INPUT = "Invalid input!";
        public const string UNAUTHORIZED = "Unauthorized!";
        public const string BADREQUEST = "Bad request!";
        public const string ERROR = "Error!";
        public const string INVALID_DATE = "INVALID_DATE";
        public const string UNPROCESSABLE_ENTITY = "Unprocessable Entity";
        public static string JWT_TOKEN_EXPIRED = "JWT Token expired";
        public const string FORBIDDEN = "FORBIDDEN";
        public const string VALIDATION_ERROR = "ValidationError";
        public const string NO_CHANGE = "You don't have permission to change the status ";

        // Payment & Membership specific codes
        public const string PAYMENT_REQUIRED = "Payment required!";
        public const string PAYMENT_SUCCESS = "Payment success!";
        public const string PAYMENT_FAILED = "Payment failed!";
        public const string ACTIVE_PACKAGE_EXISTS = "Active package exists!";
        public const string PACKAGE_NOT_FOUND = "Package not found!";
        public const string REGISTER_PACKAGE_SUCCESS = "Register package success!";
        public const string CANCEL_PACKAGE_SUCCESS = "Cancel package success!";
        public const string NO_ACTIVE_MEMBERSHIP = "No active membership!";
        public const string ALREADY_RATED = "Already rated!";
        public const string INVALID_RATING_VALUE = "Invalid rating value!";

    }

    public static class ErrorMessages
    {
        public const string NOT_FOUND = "Không tìm thấy.";
        public const string SUCCESS = "Thành công!";
        public const string FAILED = "Thất bại!";
        public const string EXISTED = "Đã tồn tại.";
        public const string DUPLICATE = "Bị trùng lặp.";
        public const string INTERNAL_SERVER_ERROR = "Lỗi máy chủ nội bộ!";
        public const string INVALID_INPUT = "Dữ liệu đầu vào không hợp lệ!";
        public const string UNAUTHORIZED = "Không có quyền truy cập!";
        public const string BADREQUEST = "Yêu cầu không hợp lệ!";
        public const string ERROR = "Lỗi!";
        public const string UNPROCESSABLE_ENTITY = "Trạng thái hiện tại không thể cập nhật thêm lần nữa";

    }
}
