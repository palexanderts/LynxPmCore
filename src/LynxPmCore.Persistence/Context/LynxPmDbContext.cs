using LynxPmCore.Domain.Aggregates.Components;
using LynxPmCore.Domain.Aggregates.Customers;
using LynxPmCore.Domain.Aggregates.Equipments;
using LynxPmCore.Domain.Aggregates.ErpSync;
using LynxPmCore.Domain.Aggregates.Notices;
using LynxPmCore.Domain.Primitives;
using LynxPmCore.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace LynxPmCore.Persistence.Context;

public sealed class LynxPmDbContext(DbContextOptions<LynxPmDbContext> options) : DbContext(options)
{
    public DbSet<Notice> Notices => Set<Notice>();
    public DbSet<Operation> Operations => Set<Operation>();
    public DbSet<NoticeCause> NoticeCauses => Set<NoticeCause>();
    public DbSet<OperationPart> OperationParts => Set<OperationPart>();
    public DbSet<Equipment> Equipments => Set<Equipment>();
    public DbSet<EquipmentMedia> EquipmentMediaItems => Set<EquipmentMedia>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<ErpSyncConfig> ErpSyncConfigs => Set<ErpSyncConfig>();
    public DbSet<ErpSyncOutboxEntry> ErpSyncOutbox => Set<ErpSyncOutboxEntry>();
    public DbSet<ComponentReceipt> ComponentReceipts => Set<ComponentReceipt>();
    public DbSet<ComponentStockLocation> ComponentStockLocations => Set<ComponentStockLocation>();
    public DbSet<ComponentUnit> ComponentUnits => Set<ComponentUnit>();
    public DbSet<ComponentMaster> ComponentMasters => Set<ComponentMaster>();
    public DbSet<ComponentNotification> ComponentNotifications => Set<ComponentNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LynxPmDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        AddOutboxMessages();
        return await base.SaveChangesAsync(ct);
    }

    private void AddOutboxMessages()
    {
        var domainEvents = ChangeTracker
            .Entries<IHasDomainEvents>()
            .Select(e => e.Entity)
            .SelectMany(e =>
            {
                var events = e.DomainEvents.ToList();
                e.ClearDomainEvents();
                return events;
            })
            .Select(e => new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = e.GetType().FullName!,
                Content = JsonConvert.SerializeObject(e, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                }),
                OccurredOnUtc = DateTime.UtcNow
            })
            .ToList();

        Set<OutboxMessage>().AddRange(domainEvents);
    }
}
