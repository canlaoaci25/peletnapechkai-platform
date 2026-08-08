using Peletnapechkai.Api.Endpoints;
using Peletnapechkai.Api.Domain.Content;

namespace Peletnapechkai.Api.Tests.Content;

public sealed class MediaUploadValidatorTests
{
    [Theory]
    [InlineData("image/jpeg", new byte[] { 0xff, 0xd8, 0xff, 0x00 }, ".jpg")]
    [InlineData("image/png", new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, ".png")]
    [InlineData("image/webp", new byte[] { 82, 73, 70, 70, 0, 0, 0, 0, 87, 69, 66, 80 }, ".webp")]
    public void Accepts_matching_image_signatures(string contentType, byte[] bytes, string expectedExtension)
    {
        Assert.True(MediaUploadValidator.TryValidate(contentType, bytes, out var extension));
        Assert.Equal(expectedExtension, extension);
    }

    [Fact]
    public void Media_asset_records_safe_optimized_variant_metadata()
    {
        var asset=new MediaAsset("2026/08/original.png","original.png","image/png",200,DateTimeOffset.UtcNow);
        asset.SetImageMetadata(2400,1350,"2026/08/cover.webp",80);
        Assert.Equal(2400,asset.Width);Assert.Equal(1350,asset.Height);Assert.Equal("2026/08/cover.webp",asset.OptimizedStorageKey);Assert.Equal(80,asset.OptimizedByteLength);
    }

    [Fact]
    public void Rejects_mime_spoofing()
    {
        Assert.False(MediaUploadValidator.TryValidate("image/png", "not an image"u8, out _));
    }
}
