using Peletnapechkai.Api.Domain.Content;

namespace Peletnapechkai.Api.Tests.Content;

public sealed class HomepagePlacementRulesTests
{
    [Fact]
    public void Rejects_more_than_four_editor_choices()
    {
        var placements = Enumerable.Range(0, 5).Select(index => ("Editors", index, Guid.NewGuid())).ToArray();
        Assert.NotNull(HomepagePlacementRules.Validate(placements));
    }

    [Fact]
    public void Rejects_multiple_leads_and_duplicate_slots()
    {
        var placements = new[] { ("Lead", 0, Guid.NewGuid()), ("Lead", 0, Guid.NewGuid()) };
        Assert.NotNull(HomepagePlacementRules.Validate(placements));
    }

    [Fact]
    public void Accepts_one_lead_and_four_editor_choices()
    {
        var placements = new List<(string, int, Guid)> { ("Lead", 0, Guid.NewGuid()) };
        placements.AddRange(Enumerable.Range(0, 4).Select(index => ("Editors", index, Guid.NewGuid())));
        Assert.Null(HomepagePlacementRules.Validate(placements));
    }
}
