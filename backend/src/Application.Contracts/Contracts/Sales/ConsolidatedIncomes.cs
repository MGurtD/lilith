using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts
{
    public class ConsolidatedIncomes:Contract
    {
        public double Year { get; set; }
        public double Month { get; set; }
        public double Week { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } = null!;
        public string TypeDetail { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Amount { get; set; }

        public string MonthKey
        {
            get
            {
                return $"{Year}-{Month}";
            }
        }

        public string WeekKey
        {
            get
            {
                return $"{Year}-{Week}";
            }
        }
    }
}
