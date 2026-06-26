namespace Application.Contracts
{
    /// <summary>
    /// Payload to update customer fiscal data fields on a SalesInvoice.
    /// Only editable while IntegrationStatusId is "Pendent" or "Error".
    /// </summary>
    public class SalesInvoiceCustomerDataUpdateDto
    {
        public string CustomerComercialName { get; set; } = string.Empty;
        public string CustomerTaxName { get; set; } = string.Empty;
        public string CustomerVatNumber { get; set; } = string.Empty;
        public string CustomerAccountNumber { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public string CustomerCity { get; set; } = string.Empty;
        public string CustomerPostalCode { get; set; } = string.Empty;
        public string CustomerRegion { get; set; } = string.Empty;
        public string CustomerCountry { get; set; } = string.Empty;
    }
}
