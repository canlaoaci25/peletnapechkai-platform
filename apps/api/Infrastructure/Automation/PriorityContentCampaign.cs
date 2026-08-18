using System.Text.Json;
using System.Security.Cryptography;

namespace Peletnapechkai.Api.Infrastructure.Automation;

public sealed record PriorityContentCampaign(string CategorySlug, int TurkishPercent, DateTimeOffset ExpiresAt)
{
    private static readonly DateTimeOffset RecipeCampaignDeadline=DateTimeOffset.Parse("2026-08-18T20:28:29.6612866+00:00");
    public static PriorityContentCampaign? RecipeFallback(DateTimeOffset now)=>now<RecipeCampaignDeadline?new("yemek-tarifleri",70,RecipeCampaignDeadline):null;

    public static PriorityContentCampaign? Load(IConfiguration configuration, DateTimeOffset now)
    {
        var path=configuration["Automation:PriorityCampaignPath"]??Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),"Peletnapechkai","AutomationWorker","priority-content-campaign.json");
        if(!File.Exists(path))return RecipeFallback(now);
        try
        {
            var value=JsonSerializer.Deserialize<PriorityContentCampaign>(File.ReadAllText(path),new JsonSerializerOptions{PropertyNameCaseInsensitive=true});
            return value is not null&&value.ExpiresAt>now&&value.TurkishPercent is >=0 and <=100&&!string.IsNullOrWhiteSpace(value.CategorySlug)?value:null;
        }
        catch(JsonException){return RecipeFallback(now);}
        catch(IOException){return RecipeFallback(now);}
        catch(UnauthorizedAccessException){return RecipeFallback(now);}
    }
    public bool SelectTurkishCuisine(Guid jobId)=>BitConverter.ToUInt32(SHA256.HashData(jobId.ToByteArray()))%100<TurkishPercent;
    public string CreateRecipeBrief(Guid jobId)
    {
        var cuisine=SelectTurkishCuisine(jobId)?"Türk mutfağından sevilen, gerçek ve uygulanabilir bir yemek":"Dünya mutfaklarından uluslararası düzeyde popüler, Türkiye'de bulunabilir malzemelere uyarlanmış bir yemek";
        return $"{cuisine} seç. Bu bir tarif içeriğidir: net porsiyon, hazırlık ve pişirme süresi, tam ölçülü malzeme listesi, gerekli ekipman, numaralı adım adım yapılış, kritik ısı/süre değerleri, kıvam ve pişme kontrolü, püf noktaları, yaygın hatalar, saklama/yeniden ısıtma ve servis önerisi zorunludur. Belirsiz 'göz kararı' anlatımdan kaçın; güvenilir yemek kaynaklarını araştır ve kopyalama yapma. Kapak bitmiş yemeği doğru göstermeli; iki gövde görseli farklı gerçek hazırlık/pişirme aşamalarını göstermeli. Görsellerde yazı, harf, rakam, logo veya filigran bulunmamalıdır.";
    }
}
