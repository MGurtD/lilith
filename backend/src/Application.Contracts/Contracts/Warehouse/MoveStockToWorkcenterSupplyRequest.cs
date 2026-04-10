using System;

namespace Application.Contracts
{
    public class MoveStockToWorkcenterSupplyRequest : Contract
    {
        public Guid StockId { get; set; }
        public Guid WorkcenterId { get; set; }
        public Guid WorkOrderPhaseId { get; set; }
        public int Quantity { get; set; }
    }
}
