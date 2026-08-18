using Peletnapechkai.Api.Domain.Content;
namespace Peletnapechkai.Api.Tests.Content;
public sealed class WebVitalSampleTests
{
 [Theory][InlineData("tr-TR","article","mobile","LCP",2499)][InlineData("en-US","home","desktop","CLS",.08)][InlineData("de-DE","category","tablet","INP",180)]
 public void Accepts_bounded_privacy_safe_dimensions(string locale,string route,string viewport,string metric,double value)=>Assert.Equal(metric,new WebVitalSample(locale,route,viewport,metric,value,DateTimeOffset.UtcNow).Metric);
 [Theory][InlineData("xx-XX","home","mobile","LCP",1)][InlineData("tr-TR","/private/url","mobile","LCP",1)][InlineData("tr-TR","home","phone","LCP",1)][InlineData("tr-TR","home","mobile","TTFB",1)][InlineData("tr-TR","home","mobile","LCP",-1)]
 public void Rejects_unbounded_or_identifying_dimensions(string locale,string route,string viewport,string metric,double value)=>Assert.ThrowsAny<ArgumentException>(()=>new WebVitalSample(locale,route,viewport,metric,value,DateTimeOffset.UtcNow));
}
