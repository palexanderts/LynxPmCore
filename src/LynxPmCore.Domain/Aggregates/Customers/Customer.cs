using LynxPmCore.Domain.Primitives;

namespace LynxPmCore.Domain.Aggregates.Customers;

public sealed class Customer : AggregateRoot
{
    private Customer() { }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Address { get; private set; }
    public string? Phone { get; private set; }
    public bool IsActive { get; private set; } = true;

    public static Customer Create(string code, string name, string? address = null, string? phone = null)
    {
        return new Customer
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name,
            Address = address,
            Phone = phone
        };
    }

    public void Update(string name, string? address, string? phone)
    {
        Name = name;
        Address = address;
        Phone = phone;
        MarkUpdated();
    }
}
