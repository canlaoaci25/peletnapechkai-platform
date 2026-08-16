using Peletnapechkai.Api.Infrastructure.Automation;

namespace Peletnapechkai.Api.Tests.Automation;

public sealed class ArticleVisualQualityPolicyTests
{
    [Fact]
    public void Accepts_relevant_credited_optimized_sixteen_nine_cover()
    {
        var result = ArticleVisualQualityPolicy.Assess(new("Android güvenlik güncellemesi", "Telefon güvenliği ve izinler",
            "<p>Android telefonlarda izin denetimi.</p>", "Android telefonda güvenlik izinlerini inceleyen kullanıcı", "BOECL özgün görsel", 1600, 900, 180_000, true));
        Assert.True(result.PassesPublicationGate);
        Assert.Empty(result.Risks);
    }

    [Fact]
    public void Rejects_missing_or_text_bearing_uncredited_cover()
    {
        var missing = ArticleVisualQualityPolicy.Assess(new("Deprem çantası", "Acil durum hazırlığı", new string('a', 1500), null, null, null, null, null, false));
        Assert.Contains("missing-cover", missing.Risks);
        Assert.Contains("missing-body-visual", missing.Risks);
        var text = ArticleVisualQualityPolicy.Assess(new("Deprem çantası", "Acil durum hazırlığı", "<p>İçerik</p>", "Büyük başlık ve logo bulunan kapak", null, 1000, 1000, null, true));
        Assert.Contains("text-risk", text.Risks);
        Assert.Contains("unsafe-crop", text.Risks);
        Assert.False(text.PassesPublicationGate);
    }
}
