using Microsoft.Extensions.Configuration;
using Peletnapechkai.Api.Infrastructure.Automation;

namespace Peletnapechkai.Api.Tests.Automation;

public sealed class PriorityContentCampaignTests
{
    [Fact]
    public void Recipe_mix_is_deterministic_and_close_to_seventy_percent_turkish()
    {
        var campaign=new PriorityContentCampaign("yemek-tarifleri",70,DateTimeOffset.MaxValue);
        var jobs=Enumerable.Range(0,10000).Select(index=>Guid.Parse($"{index:x8}-0000-0000-0000-000000000000")).ToArray();
        var turkish=jobs.Count(campaign.SelectTurkishCuisine);
        Assert.InRange(turkish,6800,7200);
        Assert.All(jobs,job=>Assert.Equal(campaign.SelectTurkishCuisine(job),campaign.SelectTurkishCuisine(job)));
        Assert.Contains("malzeme listesi",campaign.CreateRecipeBrief(jobs[0]));
        Assert.Contains("yazı",campaign.CreateRecipeBrief(jobs[0]));
    }
    [Fact]
    public void Expired_campaign_is_ignored()
    {
        var path=Path.GetTempFileName();
        try
        {
            File.WriteAllText(path,"{\"categorySlug\":\"yemek-tarifleri\",\"turkishPercent\":70,\"expiresAt\":\"2026-08-18T09:00:00Z\"}");
            var configuration=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"Automation:PriorityCampaignPath",path}}).Build();
            Assert.Null(PriorityContentCampaign.Load(configuration,DateTimeOffset.Parse("2026-08-18T10:00:00Z")));
        }
        finally{File.Delete(path);}
    }

    [Fact]
    public void Missing_file_uses_time_bounded_recipe_fallback()
    {
        var configuration=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"Automation:PriorityCampaignPath",Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".json")}}).Build();
        Assert.Equal("yemek-tarifleri",PriorityContentCampaign.Load(configuration,DateTimeOffset.Parse("2026-08-18T10:00:00Z"))?.CategorySlug);
        Assert.Null(PriorityContentCampaign.Load(configuration,DateTimeOffset.Parse("2026-08-18T21:00:00Z")));
    }
}
