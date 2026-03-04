using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalServiceTag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"INSERT INTO public.""LifecycleTags""(""Id"", ""Name"", ""Description"", ""Color"", ""Icon"", ""LifecycleId"", ""CreatedOn"", ""UpdatedOn"", ""Disabled"")
SELECT 	gen_random_uuid() as ""Id"", 
		'ExternalService' as ""Name"",
		'Servei Extern' as ""Description"",
		'success' as ""Color"",
		'pi pi-cart-plus' as ""Icon"",
		""Id"" as ""LifecycleId"",
		now() as ""CreatedOn"",
		now() as ""UpdatedOn"",
		false as ""Disabled""
FROM public.""Lifecycles""
WHERE ""Name"" = 'WorkOrder'");
        migrationBuilder.Sql(@"INSERT INTO public.""StatusLifecycleTags""(""Id"", ""StatusId"", ""LifecycleTagId"", ""CreatedOn"", ""UpdatedOn"", ""Disabled"")
VALUES(
    gen_random_uuid(),
    (SELECT ""Id"" FROM public.""Statuses"" WHERE ""Name"" = 'Servei Extern' LIMIT 1),
    (SELECT ""Id"" FROM public.""LifecycleTags"" WHERE ""Name"" = 'ExternalService' LIMIT 1),
    now(),
    now(),
    false
)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM public.""LifecycleTags"" WHERE ""Name"" = 'ExternalService'");
            migrationBuilder.Sql(@"DELETE FROM public.""StatusLifecycleTags"" WHERE ""Name"" = 'ExternalService'");
        }
    }
}
