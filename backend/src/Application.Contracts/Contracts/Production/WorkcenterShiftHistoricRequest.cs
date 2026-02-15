using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Application.Contracts
{
    public enum GroupBy
    {
        Operator,
        Workcenter,
        WorkOrder,
        Shift,
        None
    }

    public enum TimeGroupBy
    {
        Shift,
        Day,
        Week,
        Month,
        Year,
        None
    }

    public class WorkcenterShiftHistoricRequest
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GroupBy GroupBy { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TimeGroupBy TimeGroupBy { get; set; }
    }


}
