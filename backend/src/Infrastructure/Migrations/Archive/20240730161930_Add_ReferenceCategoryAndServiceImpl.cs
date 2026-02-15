using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Add_ReferenceCategoryAndServiceImpl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ServiceReferenceId",
                table: "WorkOrderPhase",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TransportCost",
                table: "WorkOrderPhase",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "ServiceReferenceId",
                table: "WorkMasterPhase",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TransportCost",
                table: "WorkMasterPhase",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ServiceCost",
                table: "SalesOrderDetail",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TransportCost",
                table: "SalesOrderDetail",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CategoryName",
                table: "References",
                type: "varchar",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TransportAmount",
                table: "References",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ServiceCost",
                table: "BudgetDetails",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TransportCost",
                table: "BudgetDetails",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "ReferenceCategories",
                columns: table => new
                {
                    Name = table.Column<string>(type: "varchar", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    Disabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferenceCategories", x => x.Name);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderPhase_ServiceReferenceId",
                table: "WorkOrderPhase",
                column: "ServiceReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkMasterPhase_ServiceReferenceId",
                table: "WorkMasterPhase",
                column: "ServiceReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_References_CategoryName",
                table: "References",
                column: "CategoryName");

            migrationBuilder.AddForeignKey(
                name: "FK_References_ReferenceCategories_CategoryName",
                table: "References",
                column: "CategoryName",
                principalTable: "ReferenceCategories",
                principalColumn: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkMasterPhase_References_ServiceReferenceId",
                table: "WorkMasterPhase",
                column: "ServiceReferenceId",
                principalTable: "References",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderPhase_References_ServiceReferenceId",
                table: "WorkOrderPhase",
                column: "ServiceReferenceId",
                principalTable: "References",
                principalColumn: "Id");

            // Create ReferenceCategories and set the category of the existing service references
            migrationBuilder.Sql("INSERT INTO \"ReferenceCategories\" (\"Name\", \"Description\", \"Disabled\")\r\n\tVALUES ('Service', 'Serveis', false), ('Material', 'Materies primeres', false), ('Tool', 'Eines', false), ('Product', 'Producte acabat', true);");
            // Reference category name update of the existing references
            migrationBuilder.Sql("UPDATE \"References\" SET \"CategoryName\" = 'Material' WHERE \"Purchase\" = true;\r\nUPDATE \"References\" SET \"CategoryName\" = 'Product' WHERE \"Sales\" = true;");

            // New version of workmaster clon procedure
            migrationBuilder.Sql($"CREATE OR REPLACE PROCEDURE public.sp_production_copyworkmaster(\r\n\tIN _referencecode character varying,\r\n\tIN _workmasterid uuid,\r\n\tIN _referenceid uuid)\r\nLANGUAGE 'plpgsql'\r\nAS $BODY$\r\n\r\n\r\nDECLARE\r\n\t_newWorkmasterId uuid;\r\n\t_newWorkmasterphaseId uuid;\r\n\t_newWorkmasterphaseDetailId uuid;\r\n\t_newWorkmasterphaseBillOfMaterialsId uuid;\r\n\t_rowWorkMasterPhase record;\r\n\t_rowWorkMasterPhaseDetail record;\r\n\t_rowWorkMasterPhaseBillOfMaterials record;\r\nBEGIN\r\n\r\n\tIF _referenceId IS NULL THEN\r\n\t\t-- Crear la referència i capturar l'uuid a _referenceId\r\n\t\t_referenceId := uuid_generate_v4();\r\n\t\tINSERT INTO public.\"References\"(\r\n\t\t\t\"Id\", \"Code\", \"Description\", \"Cost\", \"Price\", \"CreatedOn\", \"UpdatedOn\", \"Disabled\", \"Version\", \r\n\t\t\t\"TaxId\", \"Production\", \"Purchase\", \"Sales\", \"ReferenceTypeId\", \"ReferenceFormatId\", \"WorkMasterCost\", \r\n\t\t\t\"IsService\", \"LastCost\", \"CustomerId\", \"CategoryName\", \"TransportAmount\"\r\n\t\t)\r\n\t\tSELECT _referenceId, _referenceCode, \"Description\", \"Cost\", \"Price\", NOW(), NOW(),\r\n\t\t\tre.\"Disabled\", \"Version\", \"TaxId\", \"Production\", \"Purchase\", \"Sales\", \"ReferenceTypeId\",\r\n\t\t\t\"ReferenceFormatId\", \"WorkMasterCost\", \r\n\t\t\t\"IsService\", \"LastCost\", \"CustomerId\", \"CategoryName\", \"TransportAmount\"\r\n\t\tFROM public.\"References\" re\r\n\t\tINNER JOIN public.\"WorkMaster\" wo ON re.\"Id\" = wo.\"ReferenceId\"\r\n\t\tWHERE wo.\"Id\" = _workmasterId\r\n\t\tRETURNING \"Id\" INTO _referenceId;\r\n\tEND IF;\r\n\r\n\t-- Crear el workmaster i capturar l'uuid\r\n\t_newWorkmasterId := uuid_generate_v4();\r\n\tINSERT INTO public.\"WorkMaster\"(\"Id\", \"ReferenceId\", \"BaseQuantity\", \"CreatedOn\", \"UpdatedOn\", \"Disabled\", \"externalCost\", \"machineCost\", \"materialCost\", \"operatorCost\")\t\t\r\n\tSELECT _newWorkmasterId, _referenceId, \"BaseQuantity\", NOW(), NOW(), \"Disabled\", \"externalCost\", \"machineCost\", \"materialCost\", \"operatorCost\"\r\n\tFROM public.\"WorkMaster\"\r\n\tWHERE \"Id\" = _workmasterId\r\n\tRETURNING \"Id\" INTO _newWorkmasterId;\r\n\r\n\t-- Crear el Workmasterphase\r\n\tFOR _rowWorkMasterPhase IN\t\t\t\t\r\n\t\tSELECT \"Id\", \"Code\", \"Description\", \"WorkMasterId\", \"CreatedOn\", \"UpdatedOn\", \"Disabled\", \"OperatorTypeId\", \r\n\t\t\t\t\"PreferredWorkcenterId\", \"WorkcenterTypeId\", \"Comment\", \"ExternalWorkCost\", \"IsExternalWork\", \"ServiceReferenceId\", \"TransportCost\", \"ProfitPercentage\"\r\n\t\tFROM public.\"WorkMasterPhase\"\r\n\t\tWHERE \"WorkMasterId\" = _workmasterId\r\n\tLOOP\r\n\t\t_newWorkmasterphaseId := uuid_generate_v4();\r\n\t\tINSERT INTO public.\"WorkMasterPhase\"(\"Id\", \"Code\", \"Description\", \"WorkMasterId\", \"CreatedOn\", \"UpdatedOn\",\r\n\t\t\t\"Disabled\", \"OperatorTypeId\", \"PreferredWorkcenterId\", \"WorkcenterTypeId\", \"Comment\", \"ExternalWorkCost\", \"IsExternalWork\", \"ServiceReferenceId\", \"TransportCost\",\"ProfitPercentage\")\r\n\t\tVALUES (_newWorkmasterphaseId, _rowWorkMasterPhase.\"Code\", _rowWorkMasterPhase.\"Description\", _newWorkmasterId, NOW(), NOW(),\r\n\t\t\t_rowWorkMasterPhase.\"Disabled\", _rowWorkMasterPhase.\"OperatorTypeId\", _rowWorkMasterPhase.\"PreferredWorkcenterId\", _rowWorkMasterPhase.\"WorkcenterTypeId\",\r\n\t\t\t_rowWorkMasterPhase.\"Comment\", _rowWorkMasterPhase.\"ExternalWorkCost\", _rowWorkMasterPhase.\"IsExternalWork\", _rowWorkMasterPhase.\"ServiceReferenceId\", \r\n\t\t\t_rowWorkMasterPhase.\"TransportCost\", _rowWorkMasterPhase.\"ProfitPercentage\");\r\n\r\n\t\t-- Crear el WorkmasterPhaseDetail\r\n\t\tFOR _rowWorkMasterPhaseDetail IN\r\n\t\t\tSELECT \"Id\", \"WorkMasterPhaseId\", \"MachineStatusId\", \"EstimatedTime\", \"IsCycleTime\", \"CreatedOn\", \"UpdatedOn\", \"Disabled\", \"Comment\", \"Order\", \"EstimatedOperatorTime\"\t\t\t\t\t\t\t\r\n\t\t\tFROM public.\"WorkMasterPhaseDetail\"\r\n\t\t\tWHERE \"WorkMasterPhaseId\" = _rowWorkMasterPhase.\"Id\"\r\n\t\tLOOP\r\n\t\t\t_newWorkmasterphaseDetailId := uuid_generate_v4();\r\n\t\t\tINSERT INTO public.\"WorkMasterPhaseDetail\"(\"Id\", \"WorkMasterPhaseId\", \"MachineStatusId\", \"EstimatedTime\", \"IsCycleTime\", \r\n\t\t\t\t\"CreatedOn\", \"UpdatedOn\", \"Disabled\", \"Comment\", \"Order\", \"EstimatedOperatorTime\")\r\n\t\t\tVALUES (_newWorkmasterphaseDetailId, _newWorkmasterphaseId, _rowWorkMasterPhaseDetail.\"MachineStatusId\", _rowWorkMasterPhaseDetail.\"EstimatedTime\", _rowWorkMasterPhaseDetail.\"IsCycleTime\", \r\n\t\t\t\tNOW(), NOW(), _rowWorkMasterPhaseDetail.\"Disabled\", _rowWorkMasterPhaseDetail.\"Comment\", _rowWorkMasterPhaseDetail.\"Order\", _rowWorkMasterPhaseDetail.\"EstimatedOperatorTime\");\r\n\t\tEND LOOP;\r\n\r\n\t\t-- Crear el WorkmasterphaseBillOfMaterials\r\n\t\tFOR _rowWorkMasterPhaseBillOfMaterials IN\r\n\t\t\tSELECT \"Id\", \"WorkMasterPhaseId\", \"ReferenceId\", \"Quantity\", \"Width\", \"CreatedOn\", \"UpdatedOn\", \"Disabled\", \"Diameter\", \"Height\", \"Length\", \"Thickness\"\t\t\t\t\t\r\n\t\t\tFROM public.\"WorkMasterPhaseBillOfMaterials\"\r\n\t\t\tWHERE \"WorkMasterPhaseId\" = _rowWorkMasterPhase.\"Id\"\r\n\t\tLOOP\r\n\t\t\t_newWorkmasterphaseBillOfMaterialsId := uuid_generate_v4();\r\n\t\t\tINSERT INTO public.\"WorkMasterPhaseBillOfMaterials\"(\"Id\", \"WorkMasterPhaseId\", \"ReferenceId\", \"Quantity\", \"Width\", \"CreatedOn\", \"UpdatedOn\",\r\n\t\t\t\t\"Disabled\", \"Diameter\", \"Height\", \"Length\", \"Thickness\")\r\n\t\t\tVALUES (_newWorkmasterphaseBillOfMaterialsId, _newWorkmasterphaseId, _rowWorkMasterPhaseBillOfMaterials.\"ReferenceId\", _rowWorkMasterPhaseBillOfMaterials.\"Quantity\", _rowWorkMasterPhaseBillOfMaterials.\"Width\", NOW(), NOW(),\r\n\t\t\t\t_rowWorkMasterPhaseBillOfMaterials.\"Disabled\", _rowWorkMasterPhaseBillOfMaterials.\"Diameter\", _rowWorkMasterPhaseBillOfMaterials.\"Height\", _rowWorkMasterPhaseBillOfMaterials.\"Length\", _rowWorkMasterPhaseBillOfMaterials.\"Thickness\");\r\n\t\tEND LOOP;\r\n\r\n\tEND LOOP;\r\n\r\n\traise notice 'ref id: %', _referenceId;\r\n\traise notice 'new workmaster id: %', _newWorkmasterId;\r\nEND;\r\n$BODY$;\r\nALTER PROCEDURE public.sp_production_copyworkmaster(character varying, uuid, uuid)\r\n    OWNER TO ubuntu;\r\n");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_References_ReferenceCategories_CategoryName",
                table: "References");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkMasterPhase_References_ServiceReferenceId",
                table: "WorkMasterPhase");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderPhase_References_ServiceReferenceId",
                table: "WorkOrderPhase");

            migrationBuilder.DropTable(
                name: "ReferenceCategories");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrderPhase_ServiceReferenceId",
                table: "WorkOrderPhase");

            migrationBuilder.DropIndex(
                name: "IX_WorkMasterPhase_ServiceReferenceId",
                table: "WorkMasterPhase");

            migrationBuilder.DropIndex(
                name: "IX_References_CategoryName",
                table: "References");

            migrationBuilder.DropColumn(
                name: "ServiceReferenceId",
                table: "WorkOrderPhase");

            migrationBuilder.DropColumn(
                name: "TransportCost",
                table: "WorkOrderPhase");

            migrationBuilder.DropColumn(
                name: "ServiceReferenceId",
                table: "WorkMasterPhase");

            migrationBuilder.DropColumn(
                name: "TransportCost",
                table: "WorkMasterPhase");

            migrationBuilder.DropColumn(
                name: "ServiceCost",
                table: "SalesOrderDetail");

            migrationBuilder.DropColumn(
                name: "TransportCost",
                table: "SalesOrderDetail");

            migrationBuilder.DropColumn(
                name: "CategoryName",
                table: "References");

            migrationBuilder.DropColumn(
                name: "TransportAmount",
                table: "References");

            migrationBuilder.DropColumn(
                name: "ServiceCost",
                table: "BudgetDetails");

            migrationBuilder.DropColumn(
                name: "TransportCost",
                table: "BudgetDetails");
        }
    }
}
