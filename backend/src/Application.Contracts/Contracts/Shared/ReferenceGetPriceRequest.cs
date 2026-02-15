using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts
{
    public class ReferenceGetPriceRequest
    {
        public Guid referenceId { get; set; }
        public Guid supplierId { get; set; }
    }
}
