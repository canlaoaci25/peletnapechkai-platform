using Peletnapechkai.Api.Infrastructure.Automation;

namespace Peletnapechkai.Api.Tests.Automation;

public sealed class ImageTopicRelevancePolicyTests
{
    [Fact]
    public void Accepts_distinct_concrete_scenes_tied_to_the_article_topic()
    {
        Assert.True(ImageTopicRelevancePolicy.IsRelevantSet(
            "İstanbul'da deprem çantası nasıl hazırlanır?",
            "Aileler için su, fener, ilk yardım malzemeleri ve güvenli saklama önerileri.", "Afet hazırlığı",
            "ev zemininde açık deprem çantası ve acil durum malzemeleri",
            ["mutfak masasında aileler ilk yardım malzemelerini düzenliyor", "koridorda el feneri ve ilk yardım çantası kontrolü"],
            ["Deprem çantasındaki su, fener ve ilk yardım malzemeleri", "Ailenin ilk yardım malzemelerini mutfakta düzenlemesi"]));
    }

    [Theory]
    [InlineData("soyut modern dekoratif kapak görseli")]
    [InlineData("özgün yaratıcı stok fotoğraf")]
    [InlineData("deprem çantası üzerinde büyük başlık ve logo")]
    public void Rejects_generic_or_text_bearing_cover_requests(string coverQuery)
    {
        Assert.False(ImageTopicRelevancePolicy.IsRelevantSet(
            "Deprem çantası hazırlama rehberi", "Su ve ilk yardım malzemeleriyle güvenli hazırlık.", "Afet", coverQuery,
            ["aile deprem çantasına su yerleştiriyor", "ev koridorunda acil durum feneri kontrolü"],
            ["Deprem çantasına su koyan aile", "Koridorda acil durum fenerini kontrol eden kişi"]));
    }

    [Fact]
    public void Rejects_unrelated_or_repeated_body_scenes()
    {
        Assert.False(ImageTopicRelevancePolicy.IsRelevantSet(
            "Deprem çantası hazırlama rehberi", "Su ve ilk yardım malzemeleriyle güvenli hazırlık.", "Afet",
            "evde açık deprem çantası ve su şişeleri",
            ["sahilde gün batımında spor otomobil", "sahilde gün batımında spor otomobil"],
            ["Deprem çantasında su şişeleri", "İlk yardım çantası ve acil durum feneri"]));
    }

    [Fact]
    public void Accepts_distinct_recipe_cover_and_preparation_scenes()
    {
        var reasons = ImageTopicRelevancePolicy.Explain(
            "Tencerede Etli Nohut Yemeği: Yumuşak Et ve Dağılmayan Nohut İçin Tam Ölçü",
            "Etli nohut yemeğini ölçülü malzemeler, doğru ıslatma ve kontrollü pişirme adımlarıyla hazırlama rehberi.",
            "Yemek Tarifleri",
            "Beyaz tabakta bitmiş salçalı etli nohut yemeği, tane nohutlar ve kuşbaşı dana eti, doğal yemek fotoğrafı",
            [
                "Cam kâsede soğuk suda ıslanan şişmiş kuru nohutlar, gerçek hazırlık aşaması, mutfak tezgâhı",
                "Çelik tencerede salçalı sos içinde hafif kaynayan nohut ve kuşbaşı dana eti, gerçek pişirme aşaması, buhar"
            ],
            [
                "Cam kapta tuzlu soğuk su içinde geceden ıslatılan ve belirgin biçimde şişen kuru nohutlar",
                "Kalın tabanlı tencerede salçalı sos içinde hafifçe kaynayan nohutlar ve kuşbaşı dana eti"
            ]);
        Assert.True(reasons.Length == 0, string.Join(" | ", reasons));
    }
}
