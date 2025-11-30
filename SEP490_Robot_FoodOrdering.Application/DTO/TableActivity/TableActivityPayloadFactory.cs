

namespace SEP490_Robot_FoodOrdering.Application.DTO.TableActivity
{
        public static class TableActivityActions
        {
            public const string CheckoutCustomer = "CHECKOUT_CUSTOMER";
            public const string CheckoutForced = "CHECKOUT_FORCED";
            public const string CloseTimeout = "CLOSE_TIMEOUT";
        }

        public static class TableActivityPayloadFactory
        {
            public static object Build(
                string action,
                string actorType,
                Guid? actorUserId,
                string? actorDeviceId,
                string? reasonCode = null,
                string? reasonText = null,
                object? snapshot = null)
            {
                return new
                {
                    v = 2,
                    action,
                    actor = new { type = actorType, userId = actorUserId, deviceId = actorDeviceId },
                    reason = (reasonCode != null || reasonText != null) ? new { code = reasonCode, text = reasonText } : null,
                    createdAt = DateTime.UtcNow,
                    snapshot
                };
            }
        }

    }
