using LynxPmCore.Persistence.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LynxPmCore.Persistence.Processors;

public sealed class OutboxProcessor(
    LynxPmDbContext db,
    IPublisher publisher,
    ILogger<OutboxProcessor> logger)
{
    public async Task ProcessAsync(CancellationToken ct = default)
    {
        var messages = await db.OutboxMessages
            .Where(m => m.ProcessedOnUtc == null && m.RetryCount < 3)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(20)
            .ToListAsync(ct);

        foreach (var message in messages)
        {
            try
            {
                var type = Type.GetType(message.Type);
                if (type is null)
                {
                    logger.LogWarning("Unknown outbox message type: {Type}", message.Type);
                    message.ProcessedOnUtc = DateTime.UtcNow;
                    continue;
                }

                var domainEvent = (INotification?)JsonConvert.DeserializeObject(message.Content, type,
                    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

                if (domainEvent is not null)
                    await publisher.Publish(domainEvent, ct);

                message.ProcessedOnUtc = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing outbox message {Id}", message.Id);
                message.Error = ex.Message;
                message.RetryCount++;
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
