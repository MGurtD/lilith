using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateSupplyLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"INSERT INTO public.""Locations""(""Id"", ""Name"", ""Description"", ""WarehouseId"", ""CreatedOn"", ""UpdatedOn"", ""Disabled"")
                        SELECT 	gen_random_uuid() as ""Id"",
                                wc.""Name"" as ""Name"",
                                'Supply: ' || ' ' || wc.""Description"" as ""Description"",
                                wa .""Id"" as ""WarehouseId"",
                                now() as ""CreatedOn"",
                                now() as ""UpdatedOn"",
                                false as ""Disabled""	
                        FROM public.""Warehouses"" wa
                        INNER JOIN public.""Sites"" si ON wa.""SiteId"" = si.""Id""
                        INNER JOIN public.""Areas"" ar ON ar.""SiteId"" = si.""Id"" AND ar.""IsVisibleInPlant"" = true
                        INNER JOIN public.""Workcenters"" wc ON ar.""Id"" = wc.""AreaId"" AND wc.""Disabled"" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM public.""Locations"" WHERE ""Name"" LIKE 'Supply: %'");
        }
    }
}
