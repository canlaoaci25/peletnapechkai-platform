using Peletnapechkai.Api.Domain.Automation;

namespace Peletnapechkai.Api.Tests.Automation;

public sealed class AutomationCompletionPolicyTests
{
    [Theory]
    [InlineData(AutomationJobType.ContentTranslation)]
    [InlineData(AutomationJobType.SeoLocalization)]
    [InlineData(AutomationJobType.ReadyContentGeneration)]
    public void Data_jobs_cannot_complete_while_candidates_remain(AutomationJobType type)
    {
        Assert.False(AutomationCompletionPolicy.CanComplete(type, 1));
        Assert.True(AutomationCompletionPolicy.CanComplete(type, 0));
    }

    [Theory]
    [InlineData(AutomationJobType.SiteLocalization)]
    [InlineData(AutomationJobType.SystemReport)]
    public void Non_data_jobs_use_their_own_completion_evidence(AutomationJobType type)
    {
        Assert.True(AutomationCompletionPolicy.CanComplete(type, 1));
    }
}
