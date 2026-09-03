using Microsoft.Extensions.DependencyInjection;
using ProductCrud.DataServices.Audit;
using ProductCrud.DataServices.Data;
using ProductCrud.DataServices.Entities;

namespace ProductCrud.Api.BackgroundServices;

public class AuditLogBackgroundService : BackgroundService
{
    private readonly IAuditLogQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;

    public AuditLogBackgroundService(
        IAuditLogQueue queue,
        IServiceScopeFactory scopeFactory)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        await foreach (
            var message in _queue.ReadAllAsync(stoppingToken))
        {
            using var scope = _scopeFactory.CreateScope();

            var dbContext = scope.ServiceProvider
                .GetRequiredService<ProductCrudDbContext>();

            var entity = new AuditLogEntity
            {
                UserId = message.UserId,
                Username = message.Username,
                Action = message.Action,
                EntityName = message.EntityName,
                EntityId = message.EntityId,
                Description = message.Description,
                CreatedDate = message.CreatedDate
            };

            dbContext.AuditLogs.Add(entity);

            await dbContext.SaveChangesAsync(stoppingToken);
        }
    }
}