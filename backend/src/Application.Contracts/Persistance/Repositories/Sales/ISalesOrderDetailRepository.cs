using Domain.Entities.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts
{
    public interface ISalesOrderDetailRepository : IRepository<SalesOrderDetail, Guid>
    {
    }
}
