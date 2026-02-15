# External Integrations

This document describes external system integrations, primarily the Verifactu tax service for Spanish tax compliance.

## Verifactu Tax Service Integration

**Purpose:** Integration with Spanish Tax Authority (AEAT - Agencia Estatal de Administración Tributaria) for invoice registration and validation.

### Overview

```
┌──────────────────────────────────────────────────────┐
│             Lilith Backend (Api)                     │
│                                                      │
│  SalesInvoiceService                                 │
│         ↓                                            │
│  IVerifactuInvoiceService (Verifactu project)        │
│         ↓                                            │
│  SOAP Client (System.ServiceModel)                   │
│         ↓                                            │
│  X.509 Certificate Authentication                    │
│         ↓                                            │
│  HTTPS → AEAT Web Service                            │
└──────────────────────────────────────────────────────┘
```

### Key Features

- **Invoice Registration** - Submit invoices to tax authority
- **Invoice Chaining** - Hash-based integrity verification linking invoices sequentially
- **QR Code Generation** - Tax validation codes for customer verification
- **Certificate Authentication** - X.509 client certificates for secure communication
- **Invoice Search** - Query submitted invoices by various criteria

### Project Structure

**Verifactu Project (`src/Verifactu/`):**

```
Verifactu/
├── VerifactuInvoiceService.cs       # Main SOAP client
├── Contracts/
│   ├── Request/                     # Request models
│   │   ├── RegisterInvoiceRequest.cs
│   │   ├── FindInvoicesRequest.cs
│   │   └── CancelInvoiceRequest.cs
│   └── Response/                    # Response models
│       ├── VerifactuResponse.cs
│       └── FindInvoicesResponse.cs
├── Factories/
│   └── RequestFactory.cs            # Request builders
└── Mappers/
    └── InvoiceMapper.cs             # Domain to SOAP DTOs
```

### Service Interface

```csharp
public interface IVerifactuInvoiceService
{
    Task<VerifactuResponse> RegisterInvoice(RegisterInvoiceRequest request);
    Task<FindInvoicesResponse> FindInvoices(FindInvoicesRequest request);
    Task<VerifactuResponse> CancelInvoice(CancelInvoiceRequest request);
}
```

### Usage Example

```csharp
// In SalesInvoiceService
public async Task<GenericResponse> RegisterWithVerifactu(Guid invoiceId)
{
    var invoice = await unitOfWork.SalesInvoices.Get(invoiceId);
    if (invoice == null)
        return new GenericResponse(false,
            localizationService.GetLocalizedString("InvoiceNotFound", invoiceId));

    // Build Verifactu request
    var request = new RegisterInvoiceRequest
    {
        InvoiceNumber = invoice.Number,
        IssueDate = invoice.Date,
        CustomerTaxId = invoice.Customer.TaxId,
        TotalAmount = invoice.Amount,
        TaxAmount = invoice.TaxAmount,
        PreviousInvoiceHash = await GetLastInvoiceHash()  // Chain invoices
    };

    // Submit to AEAT
    var response = await verifactuService.RegisterInvoice(request);

    if (!response.Success)
        return new GenericResponse(false, response.Errors);

    // Store QR code URL
    invoice.VerifactuQrCode = response.QrCodeUrl;
    await unitOfWork.CompleteAsync();

    return new GenericResponse(true, invoice);
}
```

### Invoice Chaining Mechanism

**Purpose:** Ensures invoice integrity and prevents manipulation.

**Process:**

1. Generate hash of invoice data (number, date, amount, customer)
2. Include previous invoice's hash in current invoice
3. Submit combined data to AEAT
4. AEAT validates chain integrity

```
Invoice 1                Invoice 2                Invoice 3
   Hash A   ──────────►    Hash B   ──────────►    Hash C
                        (includes A)           (includes B)
```

**Implementation:**

```csharp
private async Task<string?> GetLastInvoiceHash()
{
    var lastInvoice = await unitOfWork.SalesInvoices
        .Find(i => !i.Disabled && i.VerifactuHash != null)
        .OrderByDescending(i => i.Date)
        .FirstOrDefaultAsync();

    return lastInvoice?.VerifactuHash;
}
```

### X.509 Certificate Configuration

**Certificate requirements:**

- Issued by Spanish Tax Authority
- Valid for client authentication
- Installed in certificate store or provided as file

**Configuration (appsettings.json):**

```json
{
  "Verifactu": {
    "CertificatePath": "path/to/certificate.pfx",
    "CertificatePassword": "password",
    "ServiceUrl": "https://prewww1.aeat.es/wlpl/TIKE-CONT/Entrada",
    "TestMode": true
  }
}
```

**Client setup:**

```csharp
var binding = new BasicHttpsBinding
{
    Security = new BasicHttpsSecurity
    {
        Mode = BasicHttpsSecurityMode.Transport,
        Transport = new HttpTransportSecurity
        {
            ClientCredentialType = HttpClientCredentialType.Certificate
        }
    }
};

var client = new VerifactuSoapClient(binding, endpoint);
client.ClientCredentials.ClientCertificate.Certificate =
    new X509Certificate2(certificatePath, certificatePassword);
```

### QR Code Generation

**Purpose:** Customers can scan QR code to verify invoice with tax authority.

**QR contains:**

- Invoice number
- Issue date
- Business tax ID
- Total amount
- Validation code from AEAT

**Usage:**

```csharp
// After successful registration
var qrUrl = response.QrCodeUrl;  // e.g., "https://aeat.es/verify?code=ABC123..."

// Store in invoice for printing/display
invoice.VerifactuQrCode = qrUrl;
```

### Error Handling

```csharp
var response = await verifactuService.RegisterInvoice(request);

if (!response.Success)
{
    // Log errors
    logger.LogError("Verifactu registration failed: {Errors}",
        string.Join(", ", response.Errors));

    // Return localized error
    return new GenericResponse(false,
        localizationService.GetLocalizedString("VerifactuRegistrationFailed"),
        response.Errors);
}
```

### Response Models

**VerifactuResponse:**

```csharp
public class VerifactuResponse
{
    public bool Success { get; set; }
    public string? QrCodeUrl { get; set; }
    public string? InvoiceHash { get; set; }
    public List<string> Errors { get; set; } = new();
    public string? ValidationCode { get; set; }
}
```

**FindInvoicesResponse:**

```csharp
public class FindInvoicesResponse
{
    public List<InvoiceInfo> Invoices { get; set; } = new();
    public int TotalCount { get; set; }
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
}
```

### Lifecycle Integration

**Verifactu lifecycle statuses:**

- `Pendent d'enviar` - Not yet submitted
- `Enviada` - Successfully registered
- `Error` - Registration failed
- `Anul·lada` - Cancelled

**Usage in SalesInvoiceService:**

```csharp
// Set initial Verifactu status
var verifactuLifecycle = unitOfWork.Lifecycles
    .Find(l => l.Name == StatusConstants.Lifecycles.Verifactu)
    .FirstOrDefault();

invoice.VerifactuStatusId = verifactuLifecycle.InitialStatusId;  // "Pendent d'enviar"

// After successful registration
var sentStatus = await unitOfWork.Lifecycles.GetStatusByName(
    StatusConstants.Lifecycles.Verifactu,
    StatusConstants.Statuses.Enviada);

invoice.VerifactuStatusId = sentStatus.Id;
```

---

## Future Integrations

### Planned

- **Email Service** - Automated invoice/order email notifications
- **Payment Gateway** - Online payment processing
- **Accounting Software** - Export to external accounting systems

### Architecture Considerations

When adding new integrations:

1. **Create separate project** if complex (like Verifactu)
2. **Define interface** in `Application.Contracts`
3. **Implement in Infrastructure** if simple
4. **Use adapter pattern** to isolate external dependencies
5. **Add configuration** in `appsettings.json`
6. **Handle errors gracefully** with localized messages
7. **Log all external calls** for debugging
8. **Consider retry policies** for transient failures

---

## Testing External Integrations

### Verifactu Testing

**Test environment:**

- Use AEAT pre-production endpoint
- Different certificate required
- Test data does not affect production

**Configuration:**

```json
{
  "Verifactu": {
    "ServiceUrl": "https://prewww1.aeat.es/wlpl/TIKE-CONT/Entrada",
    "TestMode": true
  }
}
```

### Mocking for Unit Tests

```csharp
// Mock IVerifactuInvoiceService
var mockVerifactu = new Mock<IVerifactuInvoiceService>();
mockVerifactu
    .Setup(v => v.RegisterInvoice(It.IsAny<RegisterInvoiceRequest>()))
    .ReturnsAsync(new VerifactuResponse
    {
        Success = true,
        QrCodeUrl = "https://test.url"
    });

var service = new SalesInvoiceService(unitOfWork, mockVerifactu.Object);
```

---

## Summary

### Verifactu Integration

- **Purpose:** Spanish tax compliance (AEAT invoice registration)
- **Protocol:** SOAP web service
- **Authentication:** X.509 client certificates
- **Key features:** Invoice registration, chaining, QR codes
- **Location:** Separate `Verifactu` project for isolation

### Integration Patterns

- **Separate projects** for complex integrations
- **Interface-based** for testability
- **Configuration-driven** endpoints and credentials
- **Error handling** with localized messages
- **Logging** for observability

### Related Documentation

- [Architecture Layers](architecture-layers.md) - Project organization
- [Developer Guide](developer-guide.md) - Setup and configuration
- [Localization](localization.md) - Multilanguage error messages
