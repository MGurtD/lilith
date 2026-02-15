namespace Domain.Entities
{
    public class PaymentMethod : Entity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Número de dies a afegir desde la factura
        public int DueDays { get; set; }
        // Dia de pagament
        public int PaymentDay { get; set; }

        // Número de venciments
        public int NumberOfPayments { get; set; } = 1;
        // Freqüència dels pagament
        public int Frequency { get; set; }

    }
}
