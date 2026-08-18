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
    public string? OptimizedStorageKey { get; private set; }
    public long? OptimizedByteLength { get; private set; }
    public string? PerceptualHash { get; private set; }
    public decimal? FocalX { get; private set; }
    public decimal? FocalY { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public void SetImageMetadata(int width, int height, string optimizedStorageKey, long optimizedByteLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width); ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        ArgumentException.ThrowIfNullOrWhiteSpace(optimizedStorageKey); ArgumentOutOfRangeException.ThrowIfNegativeOrZero(optimizedByteLength);
        Width=width; Height=height; OptimizedStorageKey=optimizedStorageKey.Trim(); OptimizedByteLength=optimizedByteLength;
    }

    public void SetPerceptualHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash) || hash.Length != 16 || !hash.All(Uri.IsHexDigit))
            throw new ArgumentException("Perceptual hash must contain 16 hexadecimal characters.", nameof(hash));
        PerceptualHash = hash.ToUpperInvariant();
    }

    public void SetFocalPoint(decimal focalX, decimal focalY)
    {
        if (focalX is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(focalX));
        if (focalY is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(focalY));
        FocalX = decimal.Round(focalX, 4); FocalY = decimal.Round(focalY, 4);
    }
}
