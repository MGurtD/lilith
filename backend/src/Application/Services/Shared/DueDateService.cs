using Application.Contracts;
using Application.Services;
using Domain.Entities;
using Domain.Entities.Shared;

namespace Application.Services.System
{
    public class DueDateService : IDueDateService
    {
        public List<DueDate> GenerateDueDates(PaymentMethod paymentMethod, DateTime date, decimal netAmount)
        {
            var dueDates = new List<DueDate>();

            // Factura de pagament immediat
            if (paymentMethod.PaymentDay == 0 && paymentMethod.DueDays == 0)
            {
                var due = new DueDate()
                {
                    Amount = netAmount,
                    Date = date,
                };
                dueDates.Add(due);
                return dueDates;
            }

            // Factura amb venciment
            for (var i = 0; i < paymentMethod.NumberOfPayments; i++)
            {
                var dueDateAmount = netAmount / paymentMethod.NumberOfPayments;

                var dueDays = paymentMethod.Frequency > 0 ? paymentMethod.Frequency : paymentMethod.DueDays;
                if (dueDays % 30 == 0)
                {
                    date = date.AddMonths(dueDays / 30);
                }
                else
                {
                    date = date.AddDays(dueDays);
                }

                if (paymentMethod.PaymentDay > 0)
                {
                    if (paymentMethod.PaymentDay >= date.Day)
                    {
                        // Mentre no ens passem el día de pagament, ens quedem al mateix mes associant el dia de pagament
                        date = GenerateDate(date.Year, date.Month, paymentMethod.PaymentDay);
                    } else
                    {
                        // Al passar-nos al dia de pagament, ens anem al mes següent
                        var year = date.Month == 12 ? date.Year + 1 : date.Year;
                        var month = date.Month == 12 ? 1 : date.Month + 1;

                        date = GenerateDate(year, month, paymentMethod.PaymentDay);
                    }                    
                }

                var due = new DueDate()
                {
                    Amount = decimal.Round(dueDateAmount, 2),
                    Date = date,
                };
                dueDates.Add(due);
            }

            return dueDates;
        }

        private DateTime GenerateDate(int year, int month, int day)
        {
            var daysInMonth = DateTime.DaysInMonth(year, month);
            if (daysInMonth < day) day = daysInMonth;

            return new DateTime(year, month, day);
        }

    }
}





