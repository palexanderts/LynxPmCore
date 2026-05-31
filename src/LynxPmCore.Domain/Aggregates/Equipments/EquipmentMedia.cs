using LynxPmCore.Domain.Primitives;

namespace LynxPmCore.Domain.Aggregates.Equipments;

public sealed class EquipmentMedia : BaseEntity
{
    private EquipmentMedia() { }

    public string EquipmentCode { get; private set; } = string.Empty;
    public string MediaType { get; private set; } = string.Empty;
    public string Url { get; private set; } = string.Empty;
    public string? ThumbnailUrl { get; private set; }
    public string? Title { get; private set; }
    public int Position { get; private set; }
    public string? CreatedBy { get; private set; }

    public static EquipmentMedia Create(
        string equipmentCode,
        string mediaType,
        string url,
        string? thumbnailUrl,
        string? title,
        int position,
        string? createdBy) => new()
    {
        EquipmentCode = equipmentCode.Trim().ToUpperInvariant(),
        MediaType = mediaType.Trim().ToUpperInvariant(),
        Url = url.Trim(),
        ThumbnailUrl = thumbnailUrl?.Trim(),
        Title = title?.Trim(),
        Position = position,
        CreatedBy = createdBy
    };

    public void SoftDelete() => MarkDeleted();
}
