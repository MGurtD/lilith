using System.ComponentModel.Design.Serialization;

namespace Application.Contracts
{
    /// <summary>
    /// Constants for lifecycle and status names used throughout the application.
    /// These values are stored in the database and should not be changed without corresponding database updates.
    /// </summary>
    public static class StatusConstants
    {
        // Lifecycle Names
        public static class Lifecycles
        {
            public const string Budget = "Budget";
            public const string SalesOrder = "SalesOrder";
            public const string SalesInvoice = "SalesInvoice";
            public const string DeliveryNote = "DeliveryNote";
            public const string PurchaseOrder = "PurchaseOrder";
            public const string PurchaseOrderDetail = "PurchaseOrderDetail";
            public const string PurchaseInvoice = "PurchaseInvoice";
            public const string Receipts = "Receipts";
            public const string WorkOrder = "WorkOrder";
            public const string Verifactu = "Verifactu";
        }

        // Status Names (Catalan - as stored in database)
        public static class Statuses
        {
            // Budget statuses
            public const string PendentAcceptar = "Pendent d'acceptar";
            public const string Acceptat = "Acceptat";
            public const string Rebutjat = "Rebutjat";

            // Work Order statuses
            public const string Creada = "Creada";
            public const string Production = "Producció";
            public const string Llancada = "Llançada";
            public const string Tancada = "Tancada";
            public const string OFCancellada = "Cancel·lada";
            public const string ServeiExtern = "Servei Extern";

            // Sales Order statuses
            public const string Comanda = "Comanda";
            public const string ComandaServida = "Comanda Servida";
            public const string ComandaFacturada = "Comanda Facturada";

            // Sales Invoice statuses
            public const string Cobrada = "Cobrada";

            // Purchase Order statuses
            public const string Cancellada = "Cancel·lada";
            public const string Rebuda = "Rebuda";
            public const string RebuidaParcialment = "Rebuda parcialment";
            public const string PendentRebre = "Pendent de rebre";

            // Delivery Note statuses
            public const string Entregat = "Entregat";

            // Receipt statuses
            public const string Recepcionat = "Recepcionat";

            // Verifactu statuses
            public const string Ok = "Ok";
            public const string Error = "Error";
        }

        // Site Names (Catalan - as stored in database)
        public static class Sites
        {
            public const string Temges = "Temges";
        }

        // Lifecycle Tag Names
        public static class LifecycleTags
        {
            public const string Available = "Available";
            public const string Unavailable = "Unavailable";
            public const string Plant = "Plant";

        }
    }
}
