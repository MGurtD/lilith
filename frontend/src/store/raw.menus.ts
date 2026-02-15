import { PrimeIcons } from "@primevue/core/api";
import { User } from "../types";

export const applicationMenus = [
  {
    icon: PrimeIcons.COG,
    title: "General",
    href: "",
    child: [
      {
        icon: PrimeIcons.USERS,
        title: "Usuaris",
        href: "/users",
      },
      {
        icon: PrimeIcons.CALENDAR,
        title: "Exercicis",
        href: "/exercise",
      },
      {
        icon: PrimeIcons.PERCENTAGE,
        title: "Impostos",
        href: "/taxes",
      },
      {
        icon: PrimeIcons.PAYPAL,
        title: "Formes de pagament",
        href: "/payment-methods",
      },
      {
        icon: PrimeIcons.SORT_NUMERIC_DOWN,
        title: "Sèries de factures",
        href: "/purchaseinvoiceserie",
      },
      {
        icon: PrimeIcons.REFRESH,
        title: "Cicles de vida",
        href: "/lifecycle",
      },
    ],
  },
  {
    icon: PrimeIcons.CART_PLUS,
    title: "Compres",
    href: "",
    child: [
      {
        icon: PrimeIcons.BOOKMARK,
        title: "Proveïdors",
        href: "/suppliers",
      },
      {
        icon: PrimeIcons.PALETTE,
        title: "Tipus de materials",
        href: "/referencetype",
      },
      {
        icon: PrimeIcons.TICKET,
        title: "Referències",
        href: "/material",
      },
      {
        icon: PrimeIcons.SHOPPING_BAG,
        title: "Comandes",
        child: [
          {
            icon: PrimeIcons.SHOPPING_BAG,
            title: "Comandes",
            href: "/purchase-orders",
          },
          {
            icon: PrimeIcons.SHOPPING_CART,
            title: "Comandes des de producció",
            href: "/phase-to-purchase-order",
          },
        ],
      },
      {
        icon: PrimeIcons.TRUCK,
        title: "Albarans",
        href: "/receipts",
      },
      {
        icon: PrimeIcons.MONEY_BILL,
        title: "Factures",
        href: "/purchaseinvoice",
      },
      {
        icon: PrimeIcons.WALLET,
        title: "Despeses",
        child: [
          {
            icon: PrimeIcons.TAG,
            title: "Tipus de despesa",
            href: "/expensetype",
          },
          {
            icon: PrimeIcons.WALLET,
            title: "Declaració despeses",
            href: "/expense",
          },
        ],
      },
    ],
  },
  {
    icon: PrimeIcons.MONEY_BILL,
    title: "Ventes",
    href: "",
    child: [
      {
        icon: PrimeIcons.BUILDING,
        title: "Clients",
        href: "/customers",
      },
      {
        icon: PrimeIcons.TICKET,
        title: "Referències",
        href: "/sales/reference",
      },
      {
        icon: PrimeIcons.FLAG,
        title: "Pressupostos",
        href: "/budget",
      },
      {
        icon: PrimeIcons.FLAG_FILL,
        title: "Comandes",
        href: "/salesorder",
      },
      {
        icon: PrimeIcons.TRUCK,
        title: "Albarans d'entrega",
        href: "/deliverynote",
      },
      {
        icon: PrimeIcons.WALLET,
        title: "Factures",
        href: "/sales-invoice",
      },
    ],
  },
  {
    icon: PrimeIcons.CHART_BAR,
    title: "Producció",
    href: "",
    child: [
      {
        icon: PrimeIcons.BUILDING,
        title: "Model de planta",
        child: [
          {
            icon: PrimeIcons.BUILDING,
            title: "Empresa",
            href: "/enterprise",
          },
          {
            icon: PrimeIcons.BUILDING,
            title: "Locals",
            href: "/site",
          },
          {
            icon: PrimeIcons.BUILDING,
            title: "Arees",
            href: "/area",
          },
          {
            icon: PrimeIcons.BUILDING,
            title: "Tipus de màquines",
            href: "/workcentertype",
          },
          {
            icon: PrimeIcons.BUILDING,
            title: "Màquines",
            href: "/workcenter",
          },
        ],
      },
      {
        icon: PrimeIcons.CALENDAR,
        title: "Torns",
        href: "/shifts",
      },
      {
        icon: PrimeIcons.DATABASE,
        title: "Estats de màquina",
        href: "/machinestatus",
      },
      {
        icon: PrimeIcons.EURO,
        title: "Costs de màquina",
        href: "/workcentercost",
      },
      {
        icon: PrimeIcons.USERS,
        title: "Operaris",
        child: [
          {
            icon: PrimeIcons.USERS,
            title: "Tipus d'operari",
            href: "/operatortype",
          },
          {
            icon: PrimeIcons.USER,
            title: "Operaris",
            href: "/operator",
          },
        ],
      },
      {
        icon: PrimeIcons.REPLY,
        title: "Rutes de fabricació",
        href: "/workmaster",
      },
      {
        icon: PrimeIcons.BOOK,
        title: "Ordres de fabricació",
        href: "/workorder",
      },
      {
        icon: PrimeIcons.STOPWATCH,
        title: "Tiquets de producció",
        href: "/productionpart",
      },
      {
        icon: PrimeIcons.CALENDAR_TIMES,
        title: "Histórics",
        href: "/workcentershift",
      },
    ],
  },
  {
    icon: PrimeIcons.BOX,
    title: "Magatzem",
    href: "",
    child: [
      {
        icon: PrimeIcons.BOX,
        title: "Magatzems",
        href: "/warehouse",
      },
      {
        icon: PrimeIcons.BARS,
        title: "Estocs",
        href: "/stocks",
      },
      {
        icon: PrimeIcons.ARROW_RIGHT_ARROW_LEFT,
        title: "Moviments",
        href: "/stockmovement",
      },
      {
        icon: PrimeIcons.SORT_ALT,
        title: "Inventari",
        href: "/inventory",
      },
    ],
  },
  {
    icon: PrimeIcons.CHART_PIE,
    title: "Estadístiques",
    href: "",
    child: [
      {
        icon: PrimeIcons.WALLET,
        title: "Facturació vs Despeses",
        href: "/incomesandexpensesdashboard",
      },
      {
        icon: PrimeIcons.WALLET,
        title: "Despeses",
        href: "/expense-dashboard",
      },
      {
        icon: PrimeIcons.EURO,
        title: "Costs Producció",
        href: "/productioncost",
      },
    ],
  },
  {
    icon: PrimeIcons.SHIELD,
    title: "Verifactu",
    href: "",
    child: [
      {
        icon: PrimeIcons.UPLOAD,
        title: "Integració de factures",
        href: "/verifactu/invoice-integration",
      },
      {
        icon: PrimeIcons.SEARCH,
        title: "Peticions d'integració",
        href: "/verifactu/integration-requests",
      },
      {
        icon: PrimeIcons.SEARCH_PLUS,
        title: "Consulta a Verifactu",
        href: "/verifactu/find-invoices",
      },
    ],
  },
];

export const managmentMenus = [
  {
    icon: PrimeIcons.BOOK,
    title: "Gestió de factures",
    href: "",
    child: [
      {
        icon: PrimeIcons.DOWNLOAD,
        title: "Factures de compra",
        href: "/purchaseinvoices-by-period",
      },
      {
        icon: PrimeIcons.UPLOAD,
        title: "Factures de venta",
        href: "/salesinvoices-by-period",
      },
    ],
  },
];

export const shoopflorMenus = [
  {
    icon: PrimeIcons.COG,
    title: "Taller",
    href: "/purchaseinvoices-by-period",
  },
  {
    icon: PrimeIcons.BOX,
    title: "Recepció de materials",
    href: "/purchaseinvoices-by-period",
  },
];

export const getMenusByRole = (user: User) => {
  if (user && user.username === "gestoria") {
    return managmentMenus;
  } else if (user && user.username === "planta") {
    return shoopflorMenus;
  } else {
    return applicationMenus;
  }
};
