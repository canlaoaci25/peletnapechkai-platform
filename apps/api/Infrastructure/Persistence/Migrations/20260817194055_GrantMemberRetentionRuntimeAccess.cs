using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class GrantMemberRetentionRuntimeAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'peletnapechkai_app') THEN
                        GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE saved_articles, followed_categories, article_reading_progress TO peletnapechkai_app;
                    END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'peletnapechkai_app') THEN
                        REVOKE SELECT, INSERT, UPDATE, DELETE ON TABLE saved_articles, followed_categories, article_reading_progress FROM peletnapechkai_app;
                    END IF;
                END $$;
                """);
        }
    }
}
