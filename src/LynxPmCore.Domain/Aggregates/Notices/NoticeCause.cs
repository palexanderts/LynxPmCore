using LynxPmCore.Domain.Primitives;

namespace LynxPmCore.Domain.Aggregates.Notices;

public sealed class NoticeCause : IntEntity
{
    private NoticeCause() { }

    public int NoticeId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string? Text { get; private set; }

    public static NoticeCause Create(int noticeId, string code, string? text)
    {
        return new NoticeCause
        {
            NoticeId = noticeId,
            Code = code,
            Text = text
        };
    }
}
