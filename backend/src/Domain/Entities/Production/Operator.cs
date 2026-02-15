using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Production
{
    public class Operator : Entity
    {
        public string Code { get; set; } = string.Empty; 
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string VatNumber { get; set; } = string.Empty;
        public Guid OperatorTypeId { get; set; }
        public OperatorType? OperatorType { get; set;}
    }
}
