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

        /// <summary>
        /// When true, the corrected fiscal data is also propagated to every other
        /// SalesInvoice of the same Customer that is still pending or errored on
        /// Verifactu (issue #69 follow-up). Frontend uses this after the user
        /// confirms the propagation dialog.
        /// </summary>
        public bool PropagateToAll { get; set; } = false;
    }

    /// <summary>
    /// Response payload for <c>UpdateCustomerData</c> when there are other
    /// pending/errored invoices for the same customer. The frontend uses
    /// <see cref="PendingInvoiceIds"/> to prompt the user before re-issuing the
    /// call with <see cref="SalesInvoiceCustomerDataUpdateDto.PropagateToAll"/> = true.
    /// </summary>
    public class SalesInvoiceCustomerDataUpdatePropagationResponse
    {
        public int PendingInvoicesCount { get; set; }
        public List<Guid> PendingInvoiceIds { get; set; } = new();
    }
}
