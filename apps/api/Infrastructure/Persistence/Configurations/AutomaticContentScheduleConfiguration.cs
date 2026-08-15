using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peletnapechkai.Api.Domain.Automation;
namespace Peletnapechkai.Api.Infrastructure.Persistence.Configurations;
public sealed class AutomaticContentScheduleConfiguration : IEntityTypeConfiguration<AutomaticContentSchedule>
{
    public void Configure(EntityTypeBuilder<AutomaticContentSchedule> builder)
    {
        builder.ToTable("automatic_content_schedules"); builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(item => item.IsEnabled).HasColumnName("is_enabled");
        builder.Property(item => item.IntervalMinutes).HasColumnName("interval_minutes");
        builder.Property(item => item.UpdatedByUserId).HasColumnName("updated_by_user_id");
        builder.Property(item => item.UpdatedAt).HasColumnName("updated_at");
        builder.Property(item => item.NextRunAt).HasColumnName("next_run_at");
        builder.Property(item => item.LastEnqueuedAt).HasColumnName("last_enqueued_at");
        builder.Property(item => item.LastJobId).HasColumnName("last_job_id");
    }
}
