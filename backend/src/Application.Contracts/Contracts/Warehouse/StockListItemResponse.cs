namespace Application.Contracts
{
    public class StockListItemResponse : Contract
    {
        public Guid ReferenceId { get; set; }
        public string ReferenceCode { get; set; } = string.Empty;
        public string ReferenceDescription { get; set; } = string.Empty;
        public string ReferenceDisplay { get; set; } = string.Empty;
        public Guid Id { get; set; }
        public Guid LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string LocationDescription { get; set; } = string.Empty;
        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string WarehouseDescription { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Width { get; set; }
        public decimal Length { get; set; }
        public decimal Height { get; set; }
        public decimal Diameter { get; set; }
        public decimal Thickness { get; set; }
    }
}