using System.Globalization;
using Application.Contracts;
using Application.Contracts.Migration;
using Application.Utils;
using Domain.Entities;
using Domain.Entities.Sales;

namespace Application.Services.System.Migration
{
    /// <summary>Template, export and import for customers and their address/contact auxiliary sheets.</summary>
    public class CustomerMigrationHandler(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IMigrationHandler
    {
        private const string CustomerSheet = "Customer";
        private const string AddressSheet = "CustomerAddress";
        private const string ContactSheet = "CustomerContact";
        private const string TypeSheet = "CustomerType";
        private const string PaymentSheet = "PaymentMethod";

        public string Key => "customer";
        public string DisplayNameKey => "dataMigration.entities.customer";

        public IReadOnlyList<SheetTemplate> BuildTemplate() =>
        [
            new SheetTemplate { Name = CustomerSheet, Columns = CustomerColumns() },
            new SheetTemplate { Name = AddressSheet, Columns = AddressColumns() },
            new SheetTemplate { Name = ContactSheet, Columns = ContactColumns() },
            new SheetTemplate { Name = TypeSheet, Columns = CustomerTypeColumns() },
            new SheetTemplate { Name = PaymentSheet, Columns = PaymentMethodColumns() },
        ];

        public async Task<IReadOnlyList<SheetTemplate>> BuildExport()
        {
            var allTypes = (await unitOfWork.CustomerTypes.GetAll()).ToList();
            var allPayments = (await unitOfWork.PaymentMethods.GetAll()).ToList();
            var types = allTypes.ToDictionary(t => t.Id, t => t.Name);
            var payments = allPayments.ToDictionary(p => p.Id, p => p.Name);
            var customers = (await unitOfWork.Customers.GetAll()).OrderBy(c => c.Code).ToList();

            var customerRows = new List<IReadOnlyList<string?>>();
            var addressRows = new List<IReadOnlyList<string?>>();
            var contactRows = new List<IReadOnlyList<string?>>();

            foreach (var customer in customers)
            {
                customerRows.Add(
                [
                    customer.Code,
                    customer.ComercialName,
                    customer.TaxName,
                    customer.VatNumber,
                    customer.Web,
                    customer.AccountNumber,
                    customer.Observations,
                    customer.InvoiceNotes,
                    customer.PreferredLanguage,
                    types.TryGetValue(customer.CustomerTypeId, out var typeName) ? typeName : string.Empty,
                    customer.PaymentMethodId.HasValue && payments.TryGetValue(customer.PaymentMethodId.Value, out var pmName) ? pmName : string.Empty,
                ]);

                var full = await unitOfWork.Customers.Get(customer.Id);
                if (full == null)
                    continue;

                var addressNameById = full.Address.ToDictionary(a => a.Id, a => a.Name);

                foreach (var address in full.Address.Where(a => !a.Disabled))
                {
                    addressRows.Add(
                    [
                        customer.Code,
                        address.Name,
                        address.Country,
                        address.Region,
                        address.City,
                        address.PostalCode,
                        address.Address,
                        Bool(address.Main),
                        Dec(address.DistanceFromSite),
                        Dec(address.Latitude),
                        Dec(address.Longitude),
                        address.Observations,
                    ]);
                }

                foreach (var contact in full.Contacts.Where(c => !c.Disabled))
                {
                    contactRows.Add(
                    [
                        customer.Code,
                        contact.FirstName,
                        contact.LastName,
                        contact.Charge,
                        contact.Email,
                        contact.Extension,
                        contact.PhoneNumber,
                        Bool(contact.Main),
                        contact.CustomerAddressId.HasValue && addressNameById.TryGetValue(contact.CustomerAddressId.Value, out var addrName) ? addrName : string.Empty,
                    ]);
                }
            }

            var typeRows = new List<IReadOnlyList<string?>>();
            foreach (var type in allTypes.Where(t => !t.Disabled).OrderBy(t => t.Name))
                typeRows.Add([type.Name, type.Description]);

            var paymentRows = new List<IReadOnlyList<string?>>();
            foreach (var payment in allPayments.Where(p => !p.Disabled).OrderBy(p => p.Name))
                paymentRows.Add(
                [
                    payment.Name,
                    payment.Description,
                    Int(payment.DueDays),
                    Int(payment.PaymentDay),
                    Int(payment.NumberOfPayments),
                    Int(payment.Frequency),
                ]);

            return
            [
                new SheetTemplate { Name = CustomerSheet, Columns = CustomerColumns(), Rows = customerRows },
                new SheetTemplate { Name = AddressSheet, Columns = AddressColumns(), Rows = addressRows },
                new SheetTemplate { Name = ContactSheet, Columns = ContactColumns(), Rows = contactRows },
                new SheetTemplate { Name = TypeSheet, Columns = CustomerTypeColumns(), Rows = typeRows },
                new SheetTemplate { Name = PaymentSheet, Columns = PaymentMethodColumns(), Rows = paymentRows },
            ];
        }

        public async Task<ImportReport> Import(IReadOnlyList<SheetData> sheets)
        {
            var report = new ImportReport();

            var customerSheet = sheets.FirstOrDefault(s => s.Name.Equals(CustomerSheet, StringComparison.OrdinalIgnoreCase));
            if (customerSheet == null)
            {
                report.Errors.Add(new ImportRowError
                {
                    Sheet = CustomerSheet,
                    Reason = localizationService.GetLocalizedString("MigrationSheetMissing", CustomerSheet)
                });
                return report;
            }

            var insertedTypes = await ImportCustomerTypes(sheets, report);
            var insertedPayments = await ImportPaymentMethods(sheets, report);

            var types = (await unitOfWork.CustomerTypes.GetAll())
                .GroupBy(t => t.Name.Trim().ToLowerInvariant())
                .ToDictionary(g => g.Key, g => g.First());
            var payments = (await unitOfWork.PaymentMethods.GetAll())
                .GroupBy(p => p.Name.Trim().ToLowerInvariant())
                .ToDictionary(g => g.Key, g => g.First());

            var existingCustomers = (await unitOfWork.Customers.GetAll()).ToList();
            var existingCodes = existingCustomers.Select(c => c.Code.Trim().ToLowerInvariant()).ToHashSet();
            var existingNames = existingCustomers.Select(c => c.ComercialName.Trim().ToLowerInvariant()).ToHashSet();

            var pending = new Dictionary<string, Customer>(StringComparer.OrdinalIgnoreCase);
            var pendingNames = new HashSet<string>();
            var pendingRows = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in customerSheet.Rows)
            {
                report.Total++;

                var code = row["Code"]?.Trim();
                if (string.IsNullOrWhiteSpace(code))
                {
                    Skip(report, CustomerSheet, row.Number, null, "MigrationCodeRequired");
                    continue;
                }

                if (existingCodes.Contains(code.ToLowerInvariant()) || pending.ContainsKey(code))
                {
                    Skip(report, CustomerSheet, row.Number, code, "MigrationCustomerExists", code);
                    continue;
                }

                var comercialName = row["ComercialName"]?.Trim();
                if (string.IsNullOrWhiteSpace(comercialName))
                {
                    Skip(report, CustomerSheet, row.Number, code, "MigrationComercialNameRequired");
                    continue;
                }

                if (existingNames.Contains(comercialName.ToLowerInvariant()) || pendingNames.Contains(comercialName.ToLowerInvariant()))
                {
                    Skip(report, CustomerSheet, row.Number, code, "MigrationComercialNameExists", comercialName);
                    continue;
                }

                var taxName = row["TaxName"]?.Trim() ?? string.Empty;
                var vatNumber = row["VatNumber"]?.Trim() ?? string.Empty;
                var accountNumber = row["AccountNumber"]?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(taxName))
                {
                    Skip(report, CustomerSheet, row.Number, code, "MigrationTaxNameRequired");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(vatNumber))
                {
                    Skip(report, CustomerSheet, row.Number, code, "MigrationVatNumberRequired");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(accountNumber))
                {
                    Skip(report, CustomerSheet, row.Number, code, "MigrationAccountNumberRequired");
                    continue;
                }

                if (!SpanishFiscalIdValidator.IsValidSpanishFiscalId(vatNumber))
                {
                    Skip(report, CustomerSheet, row.Number, code, "MigrationCifInvalid", vatNumber);
                    continue;
                }

                var typeName = row["CustomerType"]?.Trim();
                if (string.IsNullOrWhiteSpace(typeName) || !types.TryGetValue(typeName.ToLowerInvariant(), out var type))
                {
                    Skip(report, CustomerSheet, row.Number, code, "MigrationCustomerTypeNotFound", typeName ?? string.Empty);
                    continue;
                }

                Guid? paymentMethodId = null;
                var paymentName = row["PaymentMethod"]?.Trim();
                if (!string.IsNullOrWhiteSpace(paymentName))
                {
                    if (!payments.TryGetValue(paymentName.ToLowerInvariant(), out var payment))
                    {
                        Skip(report, CustomerSheet, row.Number, code, "MigrationPaymentMethodNotFound", paymentName);
                        continue;
                    }
                    paymentMethodId = payment.Id;
                }

                pending[code] = new Customer
                {
                    Code = code,
                    ComercialName = comercialName,
                    TaxName = taxName,
                    VatNumber = vatNumber,
                    Web = row["Web"]?.Trim() ?? string.Empty,
                    AccountNumber = accountNumber,
                    Observations = row["Observations"]?.Trim() ?? string.Empty,
                    InvoiceNotes = row["InvoiceNotes"]?.Trim() ?? string.Empty,
                    PreferredLanguage = string.IsNullOrWhiteSpace(row["PreferredLanguage"]) ? "ca" : row["PreferredLanguage"]!.Trim(),
                    CustomerTypeId = type.Id,
                    PaymentMethodId = paymentMethodId,
                };
                pendingNames.Add(comercialName.ToLowerInvariant());
                pendingRows[code] = row.Number;
            }

            var addressByKey = ImportAddresses(sheets, pending, report);
            ImportContacts(sheets, pending, addressByKey, report);

            ValidateFiscalAddresses(pending, pendingRows, report);

            if (pending.Count > 0)
                await unitOfWork.Customers.AddRange(pending.Values);

            report.Inserted = insertedTypes + insertedPayments
                + pending.Count + pending.Values.Sum(c => c.Address.Count + c.Contacts.Count);
            report.Skipped = report.Total - report.Inserted;

            return report;
        }

        private void ValidateFiscalAddresses(
            Dictionary<string, Customer> pending,
            Dictionary<string, int> pendingRows,
            ImportReport report)
        {
            foreach (var (code, customer) in pending.ToList())
            {
                var mainAddress = customer.MainAddress();
                if (mainAddress == null)
                {
                    Skip(report, CustomerSheet, pendingRows[code], code, "CustomerNoAddresses");
                    pending.Remove(code);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(mainAddress.Country)
                    || string.IsNullOrWhiteSpace(mainAddress.PostalCode)
                    || string.IsNullOrWhiteSpace(mainAddress.City)
                    || string.IsNullOrWhiteSpace(mainAddress.Address))
                {
                    Skip(report, CustomerSheet, pendingRows[code], code, "CustomerFiscalAddressInvalid");
                    pending.Remove(code);
                }
            }
        }

        private async Task<int> ImportCustomerTypes(IReadOnlyList<SheetData> sheets, ImportReport report)
        {
            var sheet = sheets.FirstOrDefault(s => s.Name.Equals(TypeSheet, StringComparison.OrdinalIgnoreCase));
            if (sheet == null)
                return 0;

            var existing = (await unitOfWork.CustomerTypes.GetAll())
                .Select(t => t.Name.Trim().ToLowerInvariant())
                .ToHashSet();
            var toAdd = new List<CustomerType>();
            var seen = new HashSet<string>();

            foreach (var row in sheet.Rows)
            {
                report.Total++;

                var name = row["Name"]?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    Skip(report, TypeSheet, row.Number, null, "MigrationNameRequired");
                    continue;
                }

                // Existing lookups are left untouched so customers can still reference them.
                var key = name.ToLowerInvariant();
                if (existing.Contains(key) || seen.Contains(key))
                    continue;

                seen.Add(key);
                toAdd.Add(new CustomerType
                {
                    Name = name,
                    Description = row["Description"]?.Trim() ?? string.Empty,
                });
            }

            if (toAdd.Count > 0)
                await unitOfWork.CustomerTypes.AddRange(toAdd);

            return toAdd.Count;
        }

        private async Task<int> ImportPaymentMethods(IReadOnlyList<SheetData> sheets, ImportReport report)
        {
            var sheet = sheets.FirstOrDefault(s => s.Name.Equals(PaymentSheet, StringComparison.OrdinalIgnoreCase));
            if (sheet == null)
                return 0;

            var existing = (await unitOfWork.PaymentMethods.GetAll())
                .Select(p => p.Name.Trim().ToLowerInvariant())
                .ToHashSet();
            var toAdd = new List<PaymentMethod>();
            var seen = new HashSet<string>();

            foreach (var row in sheet.Rows)
            {
                report.Total++;

                var name = row["Name"]?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    Skip(report, PaymentSheet, row.Number, null, "MigrationNameRequired");
                    continue;
                }

                var key = name.ToLowerInvariant();
                if (existing.Contains(key) || seen.Contains(key))
                    continue;

                if (!TryInt(row["DueDays"], 0, out var dueDays)
                    || !TryInt(row["PaymentDay"], 0, out var paymentDay)
                    || !TryInt(row["NumberOfPayments"], 1, out var numberOfPayments)
                    || !TryInt(row["Frequency"], 0, out var frequency))
                {
                    Skip(report, PaymentSheet, row.Number, name, "MigrationInvalidInteger");
                    continue;
                }

                seen.Add(key);
                toAdd.Add(new PaymentMethod
                {
                    Name = name,
                    Description = row["Description"]?.Trim() ?? string.Empty,
                    DueDays = dueDays,
                    PaymentDay = paymentDay,
                    NumberOfPayments = numberOfPayments,
                    Frequency = frequency,
                });
            }

            if (toAdd.Count > 0)
                await unitOfWork.PaymentMethods.AddRange(toAdd);

            return toAdd.Count;
        }

        private Dictionary<(string, string), CustomerAddress> ImportAddresses(
            IReadOnlyList<SheetData> sheets,
            Dictionary<string, Customer> pending,
            ImportReport report)
        {
            var addressByKey = new Dictionary<(string, string), CustomerAddress>();

            var sheet = sheets.FirstOrDefault(s => s.Name.Equals(AddressSheet, StringComparison.OrdinalIgnoreCase));
            if (sheet == null)
                return addressByKey;

            foreach (var row in sheet.Rows)
            {
                report.Total++;

                var customerCode = row["CustomerCode"]?.Trim();
                if (string.IsNullOrWhiteSpace(customerCode) || !pending.TryGetValue(customerCode, out var customer))
                {
                    Skip(report, AddressSheet, row.Number, customerCode, "MigrationParentCustomerNotFound", customerCode ?? string.Empty);
                    continue;
                }

                if (!TryDecimal(row["DistanceFromSite"], out var distance)
                    || !TryDecimal(row["Latitude"], out var latitude)
                    || !TryDecimal(row["Longitude"], out var longitude))
                {
                    Skip(report, AddressSheet, row.Number, customerCode, "MigrationInvalidDecimal");
                    continue;
                }

                var address = new CustomerAddress
                {
                    Name = row["Name"]?.Trim() ?? string.Empty,
                    Country = row["Country"]?.Trim() ?? string.Empty,
                    Region = row["Region"]?.Trim() ?? string.Empty,
                    City = row["City"]?.Trim() ?? string.Empty,
                    PostalCode = row["PostalCode"]?.Trim() ?? string.Empty,
                    Address = row["Address"]?.Trim() ?? string.Empty,
                    Main = ParseBool(row["Main"]),
                    DistanceFromSite = distance,
                    Latitude = latitude,
                    Longitude = longitude,
                    Observations = row["Observations"]?.Trim() ?? string.Empty,
                    CustomerId = customer.Id,
                };
                customer.Address.Add(address);
                addressByKey[(customerCode.ToLowerInvariant(), address.Name.Trim().ToLowerInvariant())] = address;
            }

            return addressByKey;
        }

        private void ImportContacts(
            IReadOnlyList<SheetData> sheets,
            Dictionary<string, Customer> pending,
            Dictionary<(string, string), CustomerAddress> addressByKey,
            ImportReport report)
        {
            var sheet = sheets.FirstOrDefault(s => s.Name.Equals(ContactSheet, StringComparison.OrdinalIgnoreCase));
            if (sheet == null)
                return;

            foreach (var row in sheet.Rows)
            {
                report.Total++;

                var customerCode = row["CustomerCode"]?.Trim();
                if (string.IsNullOrWhiteSpace(customerCode) || !pending.TryGetValue(customerCode, out var customer))
                {
                    Skip(report, ContactSheet, row.Number, customerCode, "MigrationParentCustomerNotFound", customerCode ?? string.Empty);
                    continue;
                }

                Guid? addressId = null;
                var addressName = row["AddressName"]?.Trim();
                if (!string.IsNullOrWhiteSpace(addressName))
                {
                    if (!addressByKey.TryGetValue((customerCode.ToLowerInvariant(), addressName.ToLowerInvariant()), out var address))
                    {
                        Skip(report, ContactSheet, row.Number, customerCode, "MigrationAddressNotFound", addressName);
                        continue;
                    }
                    addressId = address.Id;
                }

                customer.Contacts.Add(new CustomerContact
                {
                    FirstName = row["FirstName"]?.Trim() ?? string.Empty,
                    LastName = row["LastName"]?.Trim() ?? string.Empty,
                    Charge = row["Charge"]?.Trim() ?? string.Empty,
                    Email = row["Email"]?.Trim() ?? string.Empty,
                    Extension = row["Extension"]?.Trim() ?? string.Empty,
                    PhoneNumber = row["PhoneNumber"]?.Trim() ?? string.Empty,
                    Main = ParseBool(row["Main"]),
                    CustomerId = customer.Id,
                    CustomerAddressId = addressId,
                });
            }
        }

        private void Skip(ImportReport report, string sheet, int row, string? code, string reasonKey, params object[] args)
        {
            report.Errors.Add(new ImportRowError
            {
                Sheet = sheet,
                Row = row,
                Code = code,
                Reason = localizationService.GetLocalizedString(reasonKey, args)
            });
        }

        private List<SheetColumn> CustomerColumns() =>
        [
            Column("Code", "text", true),
            Column("ComercialName", "text", true),
            Column("TaxName", "text", true),
            Column("VatNumber", "text", true),
            Column("Web", "text", false),
            Column("AccountNumber", "text", true),
            Column("Observations", "text", false),
            Column("InvoiceNotes", "text", false),
            Column("PreferredLanguage", "text", false, defaultValue: "ca"),
            Column("CustomerType", "text", true, foreignKey: "CustomerType.Name"),
            Column("PaymentMethod", "text", false, foreignKey: "PaymentMethod.Name"),
        ];

        private List<SheetColumn> AddressColumns() =>
        [
            Column("CustomerCode", "text", true, foreignKey: "Customer.Code"),
            Column("Name", "text", true),
            Column("Country", "text", true),
            Column("Region", "text", false),
            Column("City", "text", true),
            Column("PostalCode", "text", true),
            Column("Address", "text", true),
            Column("Main", "boolean", false),
            Column("DistanceFromSite", "decimal", false),
            Column("Latitude", "decimal", false),
            Column("Longitude", "decimal", false),
            Column("Observations", "text", false),
        ];

        private List<SheetColumn> ContactColumns() =>
        [
            Column("CustomerCode", "text", true, foreignKey: "Customer.Code"),
            Column("FirstName", "text", false),
            Column("LastName", "text", false),
            Column("Charge", "text", false),
            Column("Email", "text", false),
            Column("Extension", "text", false),
            Column("PhoneNumber", "text", false),
            Column("Main", "boolean", false),
            Column("AddressName", "text", false, foreignKey: "CustomerAddress.Name"),
        ];

        private List<SheetColumn> CustomerTypeColumns() =>
        [
            Column("Name", "text", true),
            Column("Description", "text", false),
        ];

        private List<SheetColumn> PaymentMethodColumns() =>
        [
            Column("Name", "text", true),
            Column("Description", "text", false),
            Column("DueDays", "integer", false),
            Column("PaymentDay", "integer", false),
            Column("NumberOfPayments", "integer", false, defaultValue: "1"),
            Column("Frequency", "integer", false),
        ];

        private SheetColumn Column(string header, string type, bool required, string? foreignKey = null, string? defaultValue = null)
        {
            var typeText = localizationService.GetLocalizedString($"MigrationType_{type}");
            var requiredText = localizationService.GetLocalizedString(required ? "MigrationRequired" : "MigrationOptional");
            var comment = $"{typeText} · {requiredText}";

            if (!string.IsNullOrEmpty(foreignKey))
                comment += $" · {localizationService.GetLocalizedString("MigrationForeignKey", foreignKey)}";
            if (!string.IsNullOrEmpty(defaultValue))
                comment += $" · {localizationService.GetLocalizedString("MigrationDefault", defaultValue)}";

            return new SheetColumn { Header = header, Comment = comment };
        }

        private static string Bool(bool value) => value ? "true" : "false";

        private static string Int(int value) => value.ToString(CultureInfo.InvariantCulture);

        private static string Dec(decimal value) => value.ToString(CultureInfo.InvariantCulture);

        private static bool ParseBool(string? value)
        {
            var normalized = value?.Trim().ToLowerInvariant();
            return normalized is "true" or "1";
        }

        private static bool TryDecimal(string? value, out decimal result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = 0;
                return true;
            }

            var normalized = value.Trim().Replace(',', '.');
            return decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
        }

        private static bool TryInt(string? value, int fallback, out int result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = fallback;
                return true;
            }

            return int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        }
    }
}
