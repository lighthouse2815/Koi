using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koi.Web.Data.Migrations;

public partial class ConvertIdsToGuid : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // All statements run in the migration transaction. Keep the original foreign key
        // definitions so custom delete behavior is restored after converting both sides.
        migrationBuilder.Sql("""
            CREATE TEMP TABLE koi_guid_foreign_keys ON COMMIT DROP AS
            SELECT conrelid::regclass::text AS table_name, conname AS constraint_name,
                   pg_get_constraintdef(oid) AS definition
            FROM pg_constraint
            WHERE contype = 'f' AND connamespace = current_schema()::regnamespace
              AND (conrelid::regclass::text = ANY (ARRAY[
                '"AspNetRoleClaims"', '"AspNetUserClaims"', '"AspNetUserLogins"',
                '"AspNetUserRoles"', '"AspNetUserTokens"', '"Drinks"', '"MarketBrands"',
                '"MarketSocialLink"', '"Menus"', '"StoreCities"', '"Stores"'
              ]) OR confrelid::regclass::text = ANY (ARRAY[
                '"AspNetRoles"', '"AspNetUsers"', '"DrinkCategories"', '"Markets"',
                '"StoreCities"', '"TeaBrands"'
              ]));
            DO $$
            DECLARE fk record;
            BEGIN
              FOR fk IN SELECT * FROM koi_guid_foreign_keys LOOP
                EXECUTE format('ALTER TABLE %s DROP CONSTRAINT %I', fk.table_name, fk.constraint_name);
              END LOOP;
            END $$;
            """);

        // Encode the old integer after a table-specific prefix, preserving the order of
        // existing rows. Foreign keys use the prefix of their referenced table.
        foreach (var table in new[]
        {
            "NewsArticles", "DrinkCategories", "Drinks", "FeaturedDrinks", "Markets", "MarketSocialLink",
            "StoreCities", "TeaBrands", "Stores", "Menus", "AspNetUserClaims", "AspNetRoleClaims"
        })
        {
            migrationBuilder.Sql($"ALTER TABLE \"{table}\" ALTER COLUMN \"Id\" DROP IDENTITY IF EXISTS;");
            migrationBuilder.Sql($"ALTER TABLE \"{table}\" ALTER COLUMN \"Id\" TYPE uuid USING (substr(md5('{table}'), 1, 24) || lpad(to_hex(\"Id\"), 8, '0'))::uuid;");
        }

        foreach (var (table, column, target) in new[]
        {
            ("Drinks", "CategoryId", "DrinkCategories"), ("MarketBrands", "MarketId", "Markets"),
            ("MarketBrands", "BrandId", "TeaBrands"), ("MarketSocialLink", "MarketId", "Markets"),
            ("Menus", "MarketId", "Markets"), ("StoreCities", "MarketId", "Markets"),
            ("Stores", "MarketId", "Markets"), ("Stores", "CityId", "StoreCities"),
            ("Stores", "BrandId", "TeaBrands")
        })
            migrationBuilder.Sql($"ALTER TABLE \"{table}\" ALTER COLUMN \"{column}\" TYPE uuid USING (substr(md5('{target}'), 1, 24) || lpad(to_hex(\"{column}\"), 8, '0'))::uuid;");

        // Preserve UUID-shaped Identity keys where possible.
        // Other strings are converted consistently with their foreign keys.
        foreach (var (table, column, target) in new[]
        {
            ("AspNetUsers", "Id", "AspNetUsers"), ("AspNetRoles", "Id", "AspNetRoles"),
            ("AspNetUserClaims", "UserId", "AspNetUsers"), ("AspNetUserLogins", "UserId", "AspNetUsers"),
            ("AspNetUserRoles", "UserId", "AspNetUsers"), ("AspNetUserRoles", "RoleId", "AspNetRoles"),
            ("AspNetUserTokens", "UserId", "AspNetUsers"), ("AspNetRoleClaims", "RoleId", "AspNetRoles")
        })
            migrationBuilder.Sql($"ALTER TABLE \"{table}\" ALTER COLUMN \"{column}\" TYPE uuid USING " +
                $"CASE WHEN \"{column}\" ~* '^[0-9a-f]{{8}}-[0-9a-f]{{4}}-[0-9a-f]{{4}}-[0-9a-f]{{4}}-[0-9a-f]{{12}}$' " +
                $"THEN \"{column}\"::uuid ELSE md5('{target}:' || \"{column}\")::uuid END;");

        migrationBuilder.Sql("""
            DO $$
            DECLARE fk record;
            BEGIN
              FOR fk IN SELECT * FROM koi_guid_foreign_keys LOOP
                EXECUTE format('ALTER TABLE %s ADD CONSTRAINT %I %s', fk.table_name, fk.constraint_name, fk.definition);
              END LOOP;
            END $$;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        throw new NotSupportedException("GUID IDs cannot be converted back to integers without losing new records. Restore a backup instead.");
}
