using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWebVitalSamples : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "web_vital_samples",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    locale = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    route = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    viewport = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    metric = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    value = table.Column<double>(type: "double precision", nullable: false),
                    measured_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_web_vital_samples", x => x.id);
                    table.CheckConstraint("ck_web_vitals_locale", "locale IN ('tr-TR','en-US','de-DE','fr-FR')");
                    table.CheckConstraint("ck_web_vitals_metric", "metric IN ('LCP','CLS','INP')");
                    table.CheckConstraint("ck_web_vitals_route", "route IN ('home','article','category','search','other')");
                    table.CheckConstraint("ck_web_vitals_value", "value >= 0 AND ((metric = 'CLS' AND value <= 5) OR (metric IN ('LCP','INP') AND value <= 60000))");
                    table.CheckConstraint("ck_web_vitals_viewport", "viewport IN ('mobile','tablet','desktop')");
                });

            migrationBuilder.CreateIndex(
                name: "ix_web_vitals_window",
                table: "web_vital_samples",
                columns: new[] { "measured_at", "locale", "viewport", "metric" });
            migrationBuilder.Sql("""DO $$ BEGIN IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'peletnapechkai_app') THEN GRANT SELECT, INSERT, DELETE ON TABLE web_vital_samples TO peletnapechkai_app; END IF; END $$;""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DO $$ BEGIN IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'peletnapechkai_app') THEN REVOKE ALL PRIVILEGES ON TABLE web_vital_samples FROM peletnapechkai_app; END IF; END $$;""");
            migrationBuilder.DropTable(
                name: "web_vital_samples");
        }
    }
}
