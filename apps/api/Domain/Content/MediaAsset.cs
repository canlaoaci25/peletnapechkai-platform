namespace Peletnapechkai.Api.Domain.Content;

public sealed class MediaAsset
{
    private MediaAsset() { }

    public MediaAsset(string storageKey, string fileName, string contentType, long byteLength, DateTimeOffset createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storageKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(byteLength);
        Id = Guid.CreateVersion7();
        StorageKey = storageKey.Trim();
        FileName = fileName.Trim();
        ContentType = contentType.Trim();
        ByteLength = byteLength;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public string StorageKey { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long ByteLength { get; private set; }
    public int? Width { get; private set; }
    public int? Height { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}
