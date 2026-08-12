using System;

namespace Application.Contracts
{
    public class StockResponse : Contract
    {
        // Stock Info
        public Guid StockId { get; set; }

        // Reference Info
        public Guid ReferenceId { get; set; }
        public string ReferenceCode { get; set; } = String.Empty;
        public string ReferenceDescription { get; set; } = String.Empty;

        // Reference Format Info
        public Guid ReferenceFormatId { get; set; }
        public string ReferenceFormatCode { get; set; } = String.Empty;
        public string ReferenceFormatDescription { get; set; } = String.Empty;

        // Location Info
        public Guid LocationId { get; set; }
        public string LocationName { get; set; } = String.Empty;
        public string LocationDescription { get; set; } = String.Empty;

        // Warehouse Info
        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; } = String.Empty;
        public string WarehouseDescription { get; set; } = String.Empty;

        // Stock Quantities and Dimensions
        public decimal Quantity { get; set; }
        public decimal Width { get; set; }
        public decimal Length { get; set; }
        public decimal Height { get; set; }
        public decimal Diameter { get; set; }
        public decimal Thickness { get; set; }

        // Lot Info (traçabilitat)
        public Guid? LotId { get; set; }
        public DateTime? LotCreatedOn { get; set; }
    }
}