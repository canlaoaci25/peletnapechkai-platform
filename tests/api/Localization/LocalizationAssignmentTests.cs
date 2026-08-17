using Peletnapechkai.Api.Domain.Localization;
namespace Peletnapechkai.Api.Tests.Localization;
public sealed class LocalizationAssignmentTests
{
    [Fact] public void Assignment_tracks_owner_and_future_sla() { var now = DateTimeOffset.UtcNow; var owner = Guid.NewGuid(); var item = new LocalizationAssignment(Guid.NewGuid(), Guid.NewGuid(), owner, now.AddDays(2), Guid.NewGuid(), now); Assert.Equal(owner, item.AssigneeUserId); Assert.Equal(LocalizationAssignmentStatus.Open, item.Status); }
    [Fact] public void Reassignment_moves_work_in_progress() { var now = DateTimeOffset.UtcNow; var item = new LocalizationAssignment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), now.AddDays(2), Guid.NewGuid(), now); var next = Guid.NewGuid(); item.Assign(next, now.AddDays(3), now.AddMinutes(1)); Assert.Equal(next, item.AssigneeUserId); Assert.Equal(LocalizationAssignmentStatus.InProgress, item.Status); }
}
