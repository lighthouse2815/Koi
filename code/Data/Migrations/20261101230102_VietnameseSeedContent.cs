using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koi.Web.Data.Migrations;

public partial class VietnameseSeedContent : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        UpdateSeed(migrationBuilder);
        // Correct branches whose original imported area did not match their address.
        migrationBuilder.Sql("""
            INSERT INTO "StoreCities" ("Id", "Name", "SortOrder", "Revision") VALUES
              ('17b4123f-a3e6-f0a7-de92-f50f00002711', 'Đồng Nai', 9, 1),
              ('17b4123f-a3e6-f0a7-de92-f50f00002712', 'Bà Rịa - Vũng Tàu', 10, 1)
            ON CONFLICT ("Id") DO NOTHING;
            UPDATE "Stores" SET "CityId" = '17b4123f-a3e6-f0a7-de92-f50f00002711'
                WHERE "Id" = '821b8ee6-937c-ec96-c30f-dafb0000001a' AND "CityId" = '17b4123f-a3e6-f0a7-de92-f50f0000006b';
            UPDATE "Stores" SET "CityId" = '17b4123f-a3e6-f0a7-de92-f50f00002712'
                WHERE "Id" = '821b8ee6-937c-ec96-c30f-dafb000003f1' AND "CityId" = '17b4123f-a3e6-f0a7-de92-f50f0000006b';
            UPDATE "Stores" SET "CityId" = '17b4123f-a3e6-f0a7-de92-f50f0000006d'
                WHERE "Id" = '821b8ee6-937c-ec96-c30f-dafb000003f3' AND "CityId" = '17b4123f-a3e6-f0a7-de92-f50f0000006b';
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        throw new NotSupportedException("Restore a backup to undo the Vietnamese seed and area corrections.");

    private static void UpdateSeed(MigrationBuilder migrationBuilder)
    {
        using var stream = typeof(VietnameseSeedContent).Assembly.GetManifestResourceStream("Koi.Web.Data.SeedVietnameseUpdates.json")
            ?? throw new InvalidOperationException("Vietnamese seed updates are missing.");
        using var changes = JsonDocument.Parse(stream);
        foreach (var change in changes.RootElement.EnumerateArray())
        {
            var table = change.GetProperty("table").GetString()!;
            var column = change.GetProperty("column").GetString()!;
            var id = change.GetProperty("id").GetGuid();
            var before = change.GetProperty("old").GetString()!;
            var after = change.GetProperty("new").GetString()!;
            // Update only untouched seed values; user edits must never be overwritten.
            migrationBuilder.Sql($"UPDATE \"{table}\" SET \"{column}\" = {Literal(after)} WHERE \"Id\" = '{id}' AND \"{column}\" = {Literal(before)};");
        }
    }

    private static string Literal(string value) => "'" + value.Replace("'", "''") + "'";
}
