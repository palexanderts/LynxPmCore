using LynxPmCore.Domain.Primitives;

namespace LynxPmCore.Domain.Aggregates.Equipments;

public sealed class Equipment : AggregateRoot
{
    private Equipment() { }

    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? Location { get; private set; }
    public string? Customer { get; private set; }
    public string? ParentCode { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime? LastSyncAt { get; private set; }

    private readonly List<EquipmentMedia> _media = [];
    public IReadOnlyList<EquipmentMedia> Media => _media.AsReadOnly();

    public static Equipment Create(
        string code,
        string description,
        string? location = null,
        string? customer = null,
        string? parentCode = null)
    {
        return new Equipment
        {
            Code = code.Trim().ToUpperInvariant(),
            Description = description,
            Location = location,
            Customer = customer,
            ParentCode = parentCode
        };
    }

    public void Update(string description, string? location, string? customer)
    {
        Description = description;
        Location = location;
        Customer = customer;
        MarkUpdated();
    }

    public void MarkSynced() => LastSyncAt = DateTime.UtcNow;
}
