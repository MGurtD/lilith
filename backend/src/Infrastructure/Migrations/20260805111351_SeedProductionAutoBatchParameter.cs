using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedProductionAutoBatchParameter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"INSERT INTO public.""Parameters"" (""Id"", ""Key"", ""Value"", ""CreatedOn"", ""UpdatedOn"", ""Disabled"")
SELECT gen_random_uuid(), 'Production.AutoBatch', 'true', NOW(), NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM public.""Parameters"" WHERE ""Key"" = 'Production.AutoBatch')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM public.""Parameters"" WHERE ""Key"" = 'Production.AutoBatch'");
        }
    }
}
