using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Peletnapechkai.Api.Domain.Auditing;
using Peletnapechkai.Api.Domain.Content;
using Peletnapechkai.Api.Domain.Identity;
using Peletnapechkai.Api.Domain.Localization;
using Peletnapechkai.Api.Domain.Knowledge;

namespace Peletnapechkai.Api.Infrastructure.Persistence;

public sealed class PublishingDbContext(DbContextOptions<PublishingDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    public DbSet<Region> Regions => Set<Region>();

    public DbSet<Locale> Locales => Set<Locale>();
    public DbSet<LocaleCountry> LocaleCountries => Set<LocaleCountry>();

    public DbSet<ArticleGroup> ArticleGroups => Set<ArticleGroup>();

    public DbSet<ArticleLocalization> ArticleLocalizations => Set<ArticleLocalization>();

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Source> Sources => Set<Source>();
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
    public DbSet<ArticleRevision> ArticleRevisions => Set<ArticleRevision>();
    public DbSet<SeoMetadata> SeoMetadata => Set<SeoMetadata>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<KnowledgeCandidate> KnowledgeCandidates => Set<KnowledgeCandidate>();
    public DbSet<KnowledgeArticleLink> KnowledgeArticleLinks => Set<KnowledgeArticleLink>();
    public DbSet<EditorialTask> EditorialTasks => Set<EditorialTask>();
    public DbSet<EditorialComment> EditorialComments => Set<EditorialComment>();
    public DbSet<ArticleQualityChecklist> ArticleQualityChecklists => Set<ArticleQualityChecklist>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        EnsureAuditLogsAreAppendOnly();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        EnsureAuditLogsAreAppendOnly();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PublishingDbContext).Assembly);
        ConfigureIdentity(modelBuilder);
    }

    private static void ConfigureIdentity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationUser>(builder =>
        {
            builder.ToTable("users");
            builder.Property(user => user.Id).HasColumnName("id");
            builder.Property(user => user.UserName).HasColumnName("user_name");
            builder.Property(user => user.NormalizedUserName).HasColumnName("normalized_user_name");
            builder.Property(user => user.Email).HasColumnName("email");
            builder.Property(user => user.NormalizedEmail).HasColumnName("normalized_email");
            builder.Property(user => user.EmailConfirmed).HasColumnName("email_confirmed");
            builder.Property(user => user.PasswordHash).HasColumnName("password_hash");
            builder.Property(user => user.SecurityStamp).HasColumnName("security_stamp");
            builder.Property(user => user.ConcurrencyStamp).HasColumnName("concurrency_stamp");
            builder.Property(user => user.PhoneNumber).HasColumnName("phone_number");
            builder.Property(user => user.PhoneNumberConfirmed).HasColumnName("phone_number_confirmed");
            builder.Property(user => user.TwoFactorEnabled).HasColumnName("two_factor_enabled");
            builder.Property(user => user.LockoutEnd).HasColumnName("lockout_end");
            builder.Property(user => user.LockoutEnabled).HasColumnName("lockout_enabled");
            builder.Property(user => user.AccessFailedCount).HasColumnName("access_failed_count");
            builder.Property(user => user.DisplayName).HasColumnName("display_name").HasMaxLength(160);
            builder.Property(user => user.IsActive).HasColumnName("is_active");
            builder.Property(user => user.CreatedAt).HasColumnName("created_at");
            builder.HasIndex(user => user.NormalizedEmail).IsUnique().HasDatabaseName("ux_users_normalized_email");
            builder.HasIndex(user => user.NormalizedUserName).IsUnique().HasDatabaseName("ux_users_normalized_user_name");
        });
        modelBuilder.Entity<ApplicationRole>(builder =>
        {
            builder.ToTable("roles");
            builder.Property(role => role.Id).HasColumnName("id");
            builder.Property(role => role.Name).HasColumnName("name");
            builder.Property(role => role.NormalizedName).HasColumnName("normalized_name");
            builder.Property(role => role.ConcurrencyStamp).HasColumnName("concurrency_stamp");
            builder.HasIndex(role => role.NormalizedName).IsUnique().HasDatabaseName("ux_roles_normalized_name");
        });
        modelBuilder.Entity<IdentityUserRole<Guid>>(builder =>
        {
            builder.ToTable("user_roles");
            builder.Property(link => link.UserId).HasColumnName("user_id");
            builder.Property(link => link.RoleId).HasColumnName("role_id");
        });
        modelBuilder.Entity<IdentityUserClaim<Guid>>(builder =>
        {
            builder.ToTable("user_claims");
            builder.Property(claim => claim.Id).HasColumnName("id");
            builder.Property(claim => claim.UserId).HasColumnName("user_id");
            builder.Property(claim => claim.ClaimType).HasColumnName("claim_type");
            builder.Property(claim => claim.ClaimValue).HasColumnName("claim_value");
        });
        modelBuilder.Entity<IdentityUserLogin<Guid>>(builder =>
        {
            builder.ToTable("user_logins");
            builder.Property(login => login.LoginProvider).HasColumnName("login_provider");
            builder.Property(login => login.ProviderKey).HasColumnName("provider_key");
            builder.Property(login => login.ProviderDisplayName).HasColumnName("provider_display_name");
            builder.Property(login => login.UserId).HasColumnName("user_id");
        });
        modelBuilder.Entity<IdentityRoleClaim<Guid>>(builder =>
        {
            builder.ToTable("role_claims");
            builder.Property(claim => claim.Id).HasColumnName("id");
            builder.Property(claim => claim.RoleId).HasColumnName("role_id");
            builder.Property(claim => claim.ClaimType).HasColumnName("claim_type");
            builder.Property(claim => claim.ClaimValue).HasColumnName("claim_value");
        });
        modelBuilder.Entity<IdentityUserToken<Guid>>(builder =>
        {
            builder.ToTable("user_tokens");
            builder.Property(token => token.UserId).HasColumnName("user_id");
            builder.Property(token => token.LoginProvider).HasColumnName("login_provider");
            builder.Property(token => token.Name).HasColumnName("name");
            builder.Property(token => token.Value).HasColumnName("value");
        });

        modelBuilder.Entity<ApplicationRole>().HasData(IdentitySeedData.Roles);
    }

    private void EnsureAuditLogsAreAppendOnly()
    {
        if (ChangeTracker.Entries<AuditLog>().Any(entry =>
                entry.State is EntityState.Modified or EntityState.Deleted))
        {
            throw new InvalidOperationException("Audit log records are append-only.");
        }
    }
}
