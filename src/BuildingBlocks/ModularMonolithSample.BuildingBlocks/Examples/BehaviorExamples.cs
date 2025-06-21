using System.Data;
using MediatR;
using ModularMonolithSample.BuildingBlocks.Behaviors;

namespace ModularMonolithSample.BuildingBlocks.Examples;

/// <summary>
/// Example query that implements ICacheable for caching
/// </summary>
public record GetUserQuery(Guid UserId) : IRequest<UserDto>, ICacheable
{
    public string? CacheKey => $"User:{UserId}";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(5);
    public string[]? CacheTags => new[] { "Users" };
}

/// <summary>
/// Example command that implements multiple behaviors
/// </summary>
public record CreateOrderCommand(
    Guid CustomerId,
    List<OrderItem> Items,
    decimal Total
) : IRequest<Guid>, ITransactional, IAuditable, IRetryable
{
    // Transaction behavior
    public IsolationLevel IsolationLevel => IsolationLevel.ReadCommitted;

    // Audit behavior
    public string? Action => "CreateOrder";
    public string? Entity => "Order";
    public string? EntityId => null; // Will be set after creation
    public bool IncludeRequestData => true;

    // Retry behavior
    public int MaxRetries => 3;
    public TimeSpan BaseDelay => TimeSpan.FromMilliseconds(100);
    public Type[]? RetryableExceptions => new[] { typeof(TimeoutException), typeof(InvalidOperationException) };
}

/// <summary>
/// Example query with simple caching
/// </summary>
[Cacheable(durationMinutes: 10)]
public record GetActiveEventsQuery(int PageNumber, int PageSize) : IRequest<List<EventDto>>, ICacheable
{
    public string? CacheKey => $"ActiveEvents:{PageNumber}:{PageSize}";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
    public string[]? CacheTags => new[] { "Events", "ActiveEvents" };
}

/// <summary>
/// Example command with auditing
/// </summary>
[Auditable(entity: "User", includeRequestData: false)]
public record UpdateUserCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email
) : IRequest<bool>, IAuditable
{
    public string? Action => "UpdateUser";
    public string? Entity => "User";
    public string? EntityId => UserId.ToString();
    public bool IncludeRequestData => false; // Don't log sensitive data
}

/// <summary>
/// Example command with retry capability
/// </summary>
[Retryable(maxRetries: 5, baseDelayMs: 200)]
public record SendEmailCommand(
    string To,
    string Subject,
    string Body
) : IRequest<bool>, IRetryable
{
    public int MaxRetries => 5;
    public TimeSpan BaseDelay => TimeSpan.FromMilliseconds(200);
    public Type[]? RetryableExceptions => new[] { typeof(HttpRequestException), typeof(TaskCanceledException) };
}

/// <summary>
/// Example command with transaction
/// </summary>
[Transactional(IsolationLevel.Serializable)]
public record TransferMoneyCommand(
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount
) : IRequest<bool>, ITransactional
{
    public IsolationLevel IsolationLevel => IsolationLevel.Serializable;
}

/// <summary>
/// Complex command using multiple behaviors
/// </summary>
public record ProcessPaymentCommand(
    Guid OrderId,
    decimal Amount,
    string PaymentMethod
) : IRequest<PaymentResult>, ITransactional, IAuditable, IRetryable
{
    // Transaction - use serializable for financial operations
    public IsolationLevel IsolationLevel => IsolationLevel.Serializable;

    // Audit - log payment attempts
    public string? Action => "ProcessPayment";
    public string? Entity => "Payment";
    public string? EntityId => OrderId.ToString();
    public bool IncludeRequestData => false; // Don't log payment details

    // Retry - retry failed payments
    public int MaxRetries => 3;
    public TimeSpan BaseDelay => TimeSpan.FromSeconds(1);
    public Type[]? RetryableExceptions => new[] { typeof(HttpRequestException), typeof(TimeoutException) };
}

// DTOs for examples
public class UserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class EventDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
}

public class OrderItem
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public class PaymentResult
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string? ErrorMessage { get; set; }
} 