# SignalR Real-Time Notifications Implementation

## Overview

This document describes the SignalR implementation for real-time notifications in the Robot Food Ordering System. The system provides real-time updates for order status changes, payment status changes, and other important events to different user types (customers, kitchen staff, waiters, moderators).

## Architecture

### Components

1. **OrderNotificationHub** (`/API/Hubs/OrderNotificationHub.cs`)
   - Main SignalR hub for handling real-time connections
   - Manages group memberships for different user types
   - Provides methods for joining/leaving groups

2. **INotificationService** (`/Application/Service/Interface/INotificationService.cs`)
   - Interface defining notification methods
   - Supports different notification types (order, payment, table, staff)

3. **NotificationService** (`/Infrastructure/Notification/NotificationService.cs`)
   - Implementation of INotificationService using SignalR
   - Handles sending notifications to appropriate groups
   - Includes error handling and logging

4. **Notification DTOs** (`/Application/DTO/Response/Notification/`)
   - `OrderItemStatusNotification` - Order item status changes
   - `OrderStatusNotification` - Order status changes
   - `PaymentStatusNotification` - Payment status changes

## Configuration

### Program.cs Setup

```csharp
// Add SignalR
builder.Services.AddSignalR();

// Add SignalR notification services
builder.Services.AddSignalRNotifications();

// Map SignalR Hub
app.MapHub<OrderNotificationHub>("/orderNotificationHub");
```

### CORS Configuration

```csharp
app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true)
    .WithOrigins("http://192.168.110.46:3000", "http://localhost:5235", "https://localhost:5235")
    .AllowCredentials());
```

## Groups and Targeting

### Group Types

1. **Table Groups**: `Table_{tableId}`
   - For table-specific notifications
   - Used by customers at specific tables

2. **Kitchen Group**: `Kitchen`
   - For kitchen staff notifications
   - Receives order preparation updates

3. **Waiters Group**: `Waiters`
   - For waiter staff notifications
   - Receives serving notifications

4. **Moderators Group**: `Moderators`
   - For management/admin notifications
   - Receives all system notifications

### Group Management Methods

- `JoinTableGroup(string tableId)`
- `LeaveTableGroup(string tableId)`
- `JoinKitchenGroup()`
- `LeaveKitchenGroup()`
- `JoinWaiterGroup()`
- `LeaveWaiterGroup()`
- `JoinModeratorGroup()`
- `LeaveModeratorGroup()`

## Notification Types

### 1. Order Item Status Notifications

**Event**: `OrderItemStatusChanged`

**Triggers**: When order item status changes (Pending → Preparing → Ready → Served → Completed)

**Recipients**:
- Table group (customers)
- Kitchen group (when status involves kitchen)
- Waiters group (when item is ready to serve)
- Moderators group (always)

**Data Structure**:
```json
{
  "orderId": "guid",
  "orderItemId": "guid",
  "tableId": "guid",
  "tableName": "string",
  "productName": "string",
  "sizeName": "string",
  "oldStatus": "enum",
  "newStatus": "enum",
  "remarkNote": "string",
  "updatedAt": "datetime",
  "updatedBy": "string",
  "notificationType": "OrderItemStatusChanged"
}
```

### 2. Order Status Notifications

**Event**: `OrderStatusChanged`

**Triggers**: When overall order status changes

**Recipients**:
- Table group (customers)
- All staff groups (Kitchen, Waiters, Moderators)

**Data Structure**:
```json
{
  "orderId": "guid",
  "tableId": "guid",
  "tableName": "string",
  "oldStatus": "enum",
  "newStatus": "enum",
  "totalPrice": "decimal",
  "updatedAt": "datetime",
  "updatedBy": "string",
  "notificationType": "OrderStatusChanged"
}
```

### 3. Payment Status Notifications

**Event**: `PaymentStatusChanged`

**Triggers**: When payment status changes

**Recipients**:
- Table group (customers)
- Moderators group (for payment monitoring)

**Data Structure**:
```json
{
  "orderId": "guid",
  "tableId": "guid",
  "tableName": "string",
  "oldStatus": "enum",
  "newStatus": "enum",
  "paymentMethod": "enum",
  "totalAmount": "decimal",
  "updatedAt": "datetime",
  "notificationType": "PaymentStatusChanged"
}
```

### 4. General Notifications

**Events**: 
- `TableNotification`
- `KitchenNotification`
- `WaiterNotification`
- `ModeratorNotification`

**Data Structure**:
```json
{
  "message": "string",
  "notificationType": "string",
  "timestamp": "datetime"
}
```

## Integration Points

### OrderService Integration

The `OrderService` is integrated with the notification service to automatically send notifications when:

1. **Order Item Status Changes** (`UpdateOrderItemStatusAsync`)
   - Sends notification when item status changes
   - Sends order status notification if overall order status changes

2. **Payment Processing** (`InitiatePaymentAsync`)
   - Sends payment status notification when payment is completed

### Error Handling

- Notifications are wrapped in try-catch blocks
- Failed notifications don't impact main business operations
- Errors are logged but don't cause operation failures
- Service can be null (optional dependency)

## Client Integration

### JavaScript/TypeScript Client

```javascript
// Create connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/orderNotificationHub")
    .withAutomaticReconnect()
    .build();

// Set up event handlers
connection.on("OrderItemStatusChanged", function (notification) {
    console.log("Order item status changed:", notification);
    // Handle notification in UI
});

connection.on("OrderStatusChanged", function (notification) {
    console.log("Order status changed:", notification);
    // Handle notification in UI
});

connection.on("PaymentStatusChanged", function (notification) {
    console.log("Payment status changed:", notification);
    // Handle notification in UI
});

// Start connection
await connection.start();

// Join appropriate groups
await connection.invoke("JoinTableGroup", "table-123");
await connection.invoke("JoinKitchenGroup");
```

### Mobile Client Integration

For React Native or mobile apps, use the SignalR client libraries:

```bash
npm install @microsoft/signalr
```

Similar implementation as JavaScript but with platform-specific considerations.

## Testing

### Test Controller

A `NotificationTestController` is provided for testing purposes:

- `/api/NotificationTest/test-order-item-status`
- `/api/NotificationTest/test-order-status`
- `/api/NotificationTest/test-payment-status`
- `/api/NotificationTest/test-table-notification/{tableId}`
- `/api/NotificationTest/test-kitchen-notification`
- `/api/NotificationTest/test-waiter-notification`
- `/api/NotificationTest/test-moderator-notification`

### Test Client

A web-based test client is available at `/signalr-test.html` for testing SignalR functionality.

## Security Considerations

1. **Group Access Control**: Consider implementing authorization for group joining
2. **Rate Limiting**: Implement rate limiting for notifications
3. **Authentication**: Add user authentication/authorization to hub methods
4. **Data Validation**: Validate notification data before sending

## Performance Considerations

1. **Connection Management**: Monitor and manage connection counts
2. **Message Size**: Keep notification payloads small
3. **Group Scaling**: Consider scaling strategies for large numbers of groups
4. **Error Resilience**: Implement retry mechanisms for failed notifications

## Future Enhancements

1. **User-specific targeting**: Send notifications to specific users
2. **Message persistence**: Store notifications for offline users
3. **Push notifications**: Integrate with mobile push notification services
4. **Analytics**: Track notification delivery and engagement
5. **Customizable notifications**: Allow users to configure notification preferences

## Troubleshooting

### Common Issues

1. **Connection Failures**: Check CORS configuration and URL paths
2. **Missing Notifications**: Verify group membership and event handlers
3. **Performance Issues**: Monitor connection counts and message frequency
4. **Authentication Issues**: Ensure proper authentication setup if implemented

### Debugging

1. Use browser developer tools to monitor WebSocket connections
2. Check server logs for notification service errors
3. Use the test client to verify functionality
4. Monitor SignalR hub connection events

## Conclusion

This SignalR implementation provides a robust foundation for real-time notifications in the Robot Food Ordering System. It supports multiple user types, different notification scenarios, and includes proper error handling and testing tools.
