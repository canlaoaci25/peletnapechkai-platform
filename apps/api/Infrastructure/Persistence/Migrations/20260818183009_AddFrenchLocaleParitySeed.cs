using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peletnapechkai.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFrenchLocaleParitySeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO regions (id, code, name, currency_code, is_enabled)
                VALUES ('0198f100-0000-7000-8000-000000000004'::uuid, 'FR', 'France', 'EUR', TRUE)
                ON CONFLICT (code) DO NOTHING;

                INSERT INTO locales (id, code, language_code, region_id, display_name, native_name, is_default, is_enabled)
                SELECT '0198f100-0000-7000-9000-000000000004'::uuid, 'fr-FR', 'fr', region.id,
                       'French (France)', 'Français (France)', FALSE, TRUE
                FROM regions AS region WHERE region.code = 'FR'
                ON CONFLICT (code) DO NOTHING;

                DO $$ BEGIN
                  IF (SELECT COUNT(*) FROM locales WHERE code IN ('tr-TR','en-US','de-DE','fr-FR')) <> 4 THEN
                    RAISE EXCEPTION 'French locale parity repair did not produce all supported locales.';
                  END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Locale support is canonical platform data. A schema rollback must not remove
            // French content or relationships created after this repair.
        }
    }
}
