using System.Data;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ModularMonolithSample.BuildingBlocks.Behaviors;

/// <summary>
/// Wraps commands that implement ITransactional in database transactions
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;
    private readonly ITransactionManager _transactionManager;

    public TransactionBehavior(ILogger<TransactionBehavior<TRequest, TResponse>> logger, ITransactionManager transactionManager)
    {
        _logger = logger;
        _transactionManager = transactionManager;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Only wrap transactional requests
        if (request is not ITransactional transactionalRequest)
        {
            return await next();
        }

        var requestName = typeof(TRequest).Name;
        var isolationLevel = transactionalRequest.IsolationLevel;

        _logger.LogDebug(
            "Starting transaction for {RequestName} with isolation level {IsolationLevel}",
            requestName, isolationLevel);

        using var transaction = await _transactionManager.BeginTransactionAsync(isolationLevel, cancellationToken);
        
        try
        {
            var response = await next();
            
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogDebug(
                "Transaction committed successfully for {RequestName}",
                requestName);
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Transaction rolled back for {RequestName}. Error: {ErrorMessage}",
                requestName, ex.Message);
            
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}

/// <summary>
/// Interface for requests that require database transactions
/// </summary>
public interface ITransactional
{
    /// <summary>
    /// Transaction isolation level (default: ReadCommitted)
    /// </summary>
    IsolationLevel IsolationLevel { get; }
}

/// <summary>
/// Transaction manager abstraction for database transactions
/// </summary>
public interface ITransactionManager
{
    /// <summary>
    /// Begins a new database transaction
    /// </summary>
    Task<ITransactionScope> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a database transaction scope
/// </summary>
public interface ITransactionScope : IDisposable
{
    /// <summary>
    /// Commits the transaction
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Rolls back the transaction
    /// </summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Attribute to mark commands as transactional with default settings
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class TransactionalAttribute : Attribute
{
    public TransactionalAttribute(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        IsolationLevel = isolationLevel;
    }

    public IsolationLevel IsolationLevel { get; }
}

/// <summary>
/// Default implementation of transaction manager using Entity Framework
/// This is a placeholder - you would implement this based on your data access technology
/// </summary>
public class DefaultTransactionManager : ITransactionManager
{
    private readonly ILogger<DefaultTransactionManager> _logger;

    public DefaultTransactionManager(ILogger<DefaultTransactionManager> logger)
    {
        _logger = logger;
    }

    public async Task<ITransactionScope> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Beginning transaction with isolation level {IsolationLevel}", isolationLevel);
        
        // Placeholder implementation
        // In a real application, you would:
        // 1. Get DbContext from DI
        // 2. Begin transaction: await dbContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken)
        // 3. Return a real transaction scope
        
        return await Task.FromResult(new DefaultTransactionScope(_logger));
    }
}

/// <summary>
/// Default implementation of transaction scope
/// This is a placeholder - you would implement this based on your data access technology
/// </summary>
public class DefaultTransactionScope : ITransactionScope
{
    private readonly ILogger _logger;
    private bool _disposed = false;

    public DefaultTransactionScope(ILogger logger)
    {
        _logger = logger;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Committing transaction");
        
        // Placeholder implementation
        // In a real application: await _transaction.CommitAsync(cancellationToken);
        
        await Task.CompletedTask;
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Rolling back transaction");
        
        // Placeholder implementation
        // In a real application: await _transaction.RollbackAsync(cancellationToken);
        
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _logger.LogDebug("Disposing transaction scope");
            
            // Placeholder implementation
            // In a real application: _transaction?.Dispose();
            
            _disposed = true;
        }
    }
} 