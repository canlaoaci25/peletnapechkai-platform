using SkiaSharp;

namespace Peletnapechkai.Api.Infrastructure.Automation;

public sealed record VisualSimilarityResult(string PerceptualHash, int OriginalityScore, Guid? ClosestMediaAssetId, int ClosestSimilarityPercent);

public static class VisualSimilarityAnalyzer
{
    public static string ComputeDifferenceHash(string path)
    {
        using var source = SKBitmap.Decode(path) ?? throw new InvalidDataException("The candidate image could not be decoded.");
        using var resized = source.Resize(new SKImageInfo(9, 8, SKColorType.Gray8, SKAlphaType.Opaque), SKSamplingOptions.Default)
            ?? throw new InvalidDataException("The candidate image could not be sampled.");
        ulong hash = 0;
        for (var y = 0; y < 8; y++)
        for (var x = 0; x < 8; x++)
        {
            hash <<= 1;
            if (resized.GetPixel(x, y).Red > resized.GetPixel(x + 1, y).Red) hash |= 1;
        }
        return hash.ToString("X16");
    }

    public static int SimilarityPercent(string left, string right)
    {
        if (!ulong.TryParse(left, System.Globalization.NumberStyles.HexNumber, null, out var a) ||
            !ulong.TryParse(right, System.Globalization.NumberStyles.HexNumber, null, out var b)) return 0;
        var distance = System.Numerics.BitOperations.PopCount(a ^ b);
        return (int)Math.Round((64 - distance) / 64d * 100, MidpointRounding.AwayFromZero);
    }

    public static VisualSimilarityResult Assess(string candidateHash, IEnumerable<(Guid Id, string Hash)> archive)
    {
        var closest = archive.Select(item => (item.Id, Similarity: SimilarityPercent(candidateHash, item.Hash)))
            .OrderByDescending(item => item.Similarity).FirstOrDefault();
        var similarity = closest == default ? 0 : closest.Similarity;
        // Natural editorial images share broad luminance structure; only near-duplicates should lose the gate.
        var originality = Math.Clamp((100 - similarity) * 5, 0, 100);
        return new(candidateHash, originality, closest == default ? null : closest.Id, similarity);
    }
}
