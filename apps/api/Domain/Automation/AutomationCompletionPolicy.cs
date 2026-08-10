namespace Peletnapechkai.Api.Domain.Automation;

public static class AutomationCompletionPolicy
{
    public static bool CanComplete(AutomationJobType type, int remainingCandidates) =>
        remainingCandidates == 0 || type is not (AutomationJobType.ContentTranslation or AutomationJobType.SeoLocalization);
}
