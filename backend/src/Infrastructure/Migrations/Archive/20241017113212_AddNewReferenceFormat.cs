using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class AddNewReferenceFormat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO public.\"ReferenceFormats\"(\r\n\t\"Id\", \"Code\", \"Description\",  \"Disabled\")\r\n\tVALUES (uuid_generate_v4(), 'UNITATS', 'Unitats', false);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM public.\"ReferenceFormats\" WHERE \"Code\" = 'UNITATS';");
        }
    }
}
