using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Peletnapechkai.Api.Infrastructure.Persistence;

#nullable disable

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations;

[DbContext(typeof(PublishingDbContext))]
[Migration("20260818135000_GrantAutomationRuntimeAccess")]
public partial class GrantAutomationRuntimeAccess : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DO $$
            BEGIN
                IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'peletnapechkai_app') THEN
                    GRANT SELECT, INSERT, UPDATE, DELETE
                    ON TABLE automatic_content_schedules, article_engagements
                    TO peletnapechkai_app;
                END IF;
            END $$;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DO $$
            BEGIN
                IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'peletnapechkai_app') THEN
                    REVOKE SELECT, INSERT, UPDATE, DELETE
                    ON TABLE automatic_content_schedules, article_engagements
                    FROM peletnapechkai_app;
                END IF;
            END $$;
            """);
    }
}
