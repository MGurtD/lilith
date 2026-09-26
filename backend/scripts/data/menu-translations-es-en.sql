-- Menu item titles in Spanish (es) and English (en) - issue #144.
--
-- Fills MenuItemTranslations for the standard Lilith menu. Catalan (ca) titles
-- are never modified.
--
-- Safe to run on any environment, and more than once. For each menu item that
-- still has its standard Catalan title, the script:
--   * inserts the es/en row when the item has none;
--   * updates an es/en row only while it is untranslated: disabled, blank, or
--     identical to the Catalan title;
--   * leaves titles someone already translated by hand as they are.
-- Items whose Catalan title was renamed in that environment, and keys that do
-- not exist there, are skipped and listed so they can be translated by hand in
-- Sistema > Elements del menú > translations.
--
-- Requires PostgreSQL 13+ (gen_random_uuid()). Run it with psql, DBeaver or
-- pgAdmin against the environment's application database. Users see the new
-- titles the next time the menu loads (sign-in, reload or language change).

-- 1. Apply the translations (one statement, so it is atomic).
WITH source (menu_key, standard_catalan_title, language_code, title) AS (
    VALUES
        ('system'                     , 'Sistema'                , 'es', 'Sistema'),
        ('system'                     , 'Sistema'                , 'en', 'System'),
        ('users'                      , 'Usuaris'                , 'es', 'Usuarios'),
        ('users'                      , 'Usuaris'                , 'en', 'Users'),
        ('profiles'                   , 'Perfils d''usuari'      , 'es', 'Perfiles de usuario'),
        ('profiles'                   , 'Perfils d''usuari'      , 'en', 'User profiles'),
        ('menuitems'                  , 'Elements del menú'      , 'es', 'Elementos del menú'),
        ('menuitems'                  , 'Elements del menú'      , 'en', 'Menu items'),
        ('lifecycle'                  , 'Cicles de vida'         , 'es', 'Ciclos de vida'),
        ('lifecycle'                  , 'Cicles de vida'         , 'en', 'Lifecycles'),
        ('apikeys'                    , 'Api keys'               , 'es', 'Claves de API'),
        ('apikeys'                    , 'Api keys'               , 'en', 'API keys'),
        ('reports'                    , 'Informes'               , 'es', 'Informes'),
        ('reports'                    , 'Informes'               , 'en', 'Reports'),
        ('application_branding'       , 'Branding'               , 'es', 'Marca'),
        ('application_branding'       , 'Branding'               , 'en', 'Branding'),
        ('general'                    , 'General'                , 'es', 'General'),
        ('general'                    , 'General'                , 'en', 'General'),
        ('exercise'                   , 'Exercicis'              , 'es', 'Ejercicios'),
        ('exercise'                   , 'Exercicis'              , 'en', 'Fiscal years'),
        ('taxes'                      , 'Impostos'               , 'es', 'Impuestos'),
        ('taxes'                      , 'Impostos'               , 'en', 'Taxes'),
        ('paymentmethods'             , 'Formes de pagament'     , 'es', 'Formas de pago'),
        ('paymentmethods'             , 'Formes de pagament'     , 'en', 'Payment methods'),
        ('invoiceseries'              , 'Sèries de factures'     , 'es', 'Series de facturas'),
        ('invoiceseries'              , 'Sèries de factures'     , 'en', 'Invoice series'),
        ('purchase'                   , 'Compres'                , 'es', 'Compras'),
        ('purchase'                   , 'Compres'                , 'en', 'Purchasing'),
        ('suppliers'                  , 'Proveïdors'             , 'es', 'Proveedores'),
        ('suppliers'                  , 'Proveïdors'             , 'en', 'Suppliers'),
        ('referencetype'              , 'Tipus de materials'     , 'es', 'Tipos de materiales'),
        ('referencetype'              , 'Tipus de materials'     , 'en', 'Material types'),
        ('material'                   , 'Referències'            , 'es', 'Referencias'),
        ('material'                   , 'Referències'            , 'en', 'References'),
        ('purchase_orders_group'      , 'Comandes'               , 'es', 'Pedidos'),
        ('purchase_orders_group'      , 'Comandes'               , 'en', 'Orders'),
        ('purchase_orders'            , 'Comandes'               , 'es', 'Pedidos'),
        ('purchase_orders'            , 'Comandes'               , 'en', 'Orders'),
        ('phase_to_purchase_order'    , 'Comandes de producció'  , 'es', 'Pedidos de producción'),
        ('phase_to_purchase_order'    , 'Comandes de producció'  , 'en', 'Production purchase orders'),
        ('receipts'                   , 'Albarans'               , 'es', 'Albaranes'),
        ('receipts'                   , 'Albarans'               , 'en', 'Goods receipts'),
        ('purchaseinvoice'            , 'Factures'               , 'es', 'Facturas'),
        ('purchaseinvoice'            , 'Factures'               , 'en', 'Invoices'),
        ('expenses_group'             , 'Despeses'               , 'es', 'Gastos'),
        ('expenses_group'             , 'Despeses'               , 'en', 'Expenses'),
        ('expensetype'                , 'Tipus de despesa'       , 'es', 'Tipos de gasto'),
        ('expensetype'                , 'Tipus de despesa'       , 'en', 'Expense types'),
        ('expense'                    , 'Declaració despeses'    , 'es', 'Declaración de gastos'),
        ('expense'                    , 'Declaració despeses'    , 'en', 'Expense statements'),
        ('sales'                      , 'Ventes'                 , 'es', 'Ventas'),
        ('sales'                      , 'Ventes'                 , 'en', 'Sales'),
        ('customers'                  , 'Clients'                , 'es', 'Clientes'),
        ('customers'                  , 'Clients'                , 'en', 'Customers'),
        ('sales_reference'            , 'Referències'            , 'es', 'Referencias'),
        ('sales_reference'            , 'Referències'            , 'en', 'References'),
        ('budget'                     , 'Pressupostos'           , 'es', 'Presupuestos'),
        ('budget'                     , 'Pressupostos'           , 'en', 'Quotes'),
        ('salesorder'                 , 'Comandes'               , 'es', 'Pedidos'),
        ('salesorder'                 , 'Comandes'               , 'en', 'Orders'),
        ('deliverynote'               , 'Albarans d''entrega'    , 'es', 'Albaranes de entrega'),
        ('deliverynote'               , 'Albarans d''entrega'    , 'en', 'Delivery notes'),
        ('sales_invoice'              , 'Factures'               , 'es', 'Facturas'),
        ('sales_invoice'              , 'Factures'               , 'en', 'Invoices'),
        ('production'                 , 'Producció'              , 'es', 'Producción'),
        ('production'                 , 'Producció'              , 'en', 'Production'),
        ('plantmodel'                 , 'Model de planta'        , 'es', 'Modelo de planta'),
        ('plantmodel'                 , 'Model de planta'        , 'en', 'Plant model'),
        ('enterprise'                 , 'Empresa'                , 'es', 'Empresa'),
        ('enterprise'                 , 'Empresa'                , 'en', 'Company'),
        ('site'                       , 'Locals'                 , 'es', 'Locales'),
        ('site'                       , 'Locals'                 , 'en', 'Sites'),
        ('area'                       , 'Arees'                  , 'es', 'Áreas'),
        ('area'                       , 'Arees'                  , 'en', 'Areas'),
        ('workcentertype'             , 'Tipus de màquines'      , 'es', 'Tipos de máquinas'),
        ('workcentertype'             , 'Tipus de màquines'      , 'en', 'Machine types'),
        ('workcenter'                 , 'Màquines'               , 'es', 'Máquinas'),
        ('workcenter'                 , 'Màquines'               , 'en', 'Machines'),
        ('shifts'                     , 'Torns'                  , 'es', 'Turnos'),
        ('shifts'                     , 'Torns'                  , 'en', 'Shifts'),
        ('machinestatus'              , 'Estats de màquina'      , 'es', 'Estados de máquina'),
        ('machinestatus'              , 'Estats de màquina'      , 'en', 'Machine statuses'),
        ('workcentercost'             , 'Costs de màquina'       , 'es', 'Costes de máquina'),
        ('workcentercost'             , 'Costs de màquina'       , 'en', 'Machine costs'),
        ('operators_group'            , 'Operaris'               , 'es', 'Operarios'),
        ('operators_group'            , 'Operaris'               , 'en', 'Operators'),
        ('operatortype'               , 'Tipus d''operari'       , 'es', 'Tipos de operario'),
        ('operatortype'               , 'Tipus d''operari'       , 'en', 'Operator types'),
        ('operator'                   , 'Operaris'               , 'es', 'Operarios'),
        ('operator'                   , 'Operaris'               , 'en', 'Operators'),
        ('phasetemplate'              , 'Plantilla de fase'      , 'es', 'Plantillas de fase'),
        ('phasetemplate'              , 'Plantilla de fase'      , 'en', 'Phase templates'),
        ('workmaster'                 , 'Rutes de fabricació'    , 'es', 'Rutas de fabricación'),
        ('workmaster'                 , 'Rutes de fabricació'    , 'en', 'Manufacturing routes'),
        ('workorder'                  , 'Ordres de fabricació'   , 'es', 'Órdenes de fabricación'),
        ('workorder'                  , 'Ordres de fabricació'   , 'en', 'Manufacturing orders'),
        ('productionpart'             , 'Prioritzar OFs'         , 'es', 'Priorizar OFs'),
        ('productionpart'             , 'Prioritzar OFs'         , 'en', 'Prioritize manufacturing orders'),
        ('workcentersaturation'       , 'Saturació'              , 'es', 'Saturación'),
        ('workcentersaturation'       , 'Saturació'              , 'en', 'Saturation'),
        ('workcentershift'            , 'Històric de producció'  , 'es', 'Histórico de producción'),
        ('workcentershift'            , 'Històric de producció'  , 'en', 'Production history'),
        ('warehouse'                  , 'Magatzem'               , 'es', 'Almacén'),
        ('warehouse'                  , 'Magatzem'               , 'en', 'Warehouse'),
        ('warehouse_list'             , 'Magatzems'              , 'es', 'Almacenes'),
        ('warehouse_list'             , 'Magatzems'              , 'en', 'Warehouses'),
        ('stocks'                     , 'Estocs'                 , 'es', 'Stocks'),
        ('stocks'                     , 'Estocs'                 , 'en', 'Stock'),
        ('stockmovement'              , 'Moviments'              , 'es', 'Movimientos'),
        ('stockmovement'              , 'Moviments'              , 'en', 'Stock movements'),
        ('inventory'                  , 'Inventari'              , 'es', 'Inventario'),
        ('inventory'                  , 'Inventari'              , 'en', 'Inventory'),
        ('statistics'                 , 'Estadístiques'          , 'es', 'Estadísticas'),
        ('statistics'                 , 'Estadístiques'          , 'en', 'Statistics'),
        ('Production Dashboard'       , 'Dashboard de producció' , 'es', 'Panel de producción'),
        ('Production Dashboard'       , 'Dashboard de producció' , 'en', 'Production dashboard'),
        ('analytics_customers_ranking', 'Ranking Clients'        , 'es', 'Ranking de clientes'),
        ('analytics_customers_ranking', 'Ranking Clients'        , 'en', 'Customer ranking'),
        ('incomes_vs_expenses'        , 'Facturació vs Despeses' , 'es', 'Facturación vs. gastos'),
        ('incomes_vs_expenses'        , 'Facturació vs Despeses' , 'en', 'Revenue vs. expenses'),
        ('expense_dashboard'          , 'Despeses'               , 'es', 'Gastos'),
        ('expense_dashboard'          , 'Despeses'               , 'en', 'Expenses'),
        ('productioncost'             , 'Costs Producció'        , 'es', 'Costes de producción'),
        ('productioncost'             , 'Costs Producció'        , 'en', 'Production costs'),
        ('verifactu'                  , 'Verifactu'              , 'es', 'Verifactu'),
        ('verifactu'                  , 'Verifactu'              , 'en', 'Verifactu'),
        ('invoice_integration'        , 'Integració de factures' , 'es', 'Integración de facturas'),
        ('invoice_integration'        , 'Integració de factures' , 'en', 'Invoice integration'),
        ('integration_requests'       , 'Peticions d''integració', 'es', 'Peticiones de integración'),
        ('integration_requests'       , 'Peticions d''integració', 'en', 'Integration requests'),
        ('find_invoices'              , 'Consulta a Verifactu'   , 'es', 'Consulta a Verifactu'),
        ('find_invoices'              , 'Consulta a Verifactu'   , 'en', 'Verifactu lookup'),
        ('management_root'            , 'Gestió de factures'     , 'es', 'Gestión de facturas'),
        ('management_root'            , 'Gestió de factures'     , 'en', 'Invoice management'),
        ('management_purchaseinvoices', 'Factures de compra'     , 'es', 'Facturas de compra'),
        ('management_purchaseinvoices', 'Factures de compra'     , 'en', 'Purchase invoices'),
        ('management_salesinvoices'   , 'Factures de venta'      , 'es', 'Facturas de venta'),
        ('management_salesinvoices'   , 'Factures de venta'      , 'en', 'Sales invoices'),
        ('shopfloor_root'             , 'Planta'                 , 'es', 'Planta'),
        ('shopfloor_root'             , 'Planta'                 , 'en', 'Plant')
),
matched AS (
    SELECT s.menu_key, s.language_code, s.title, m."Id" AS menu_item_id,
           ca."Title" AS catalan_title, s.standard_catalan_title
    FROM source s
    JOIN public."MenuItems" m ON m."Key" = s.menu_key
    LEFT JOIN public."MenuItemTranslations" ca
        ON ca."MenuItemId" = m."Id" AND lower(ca."LanguageCode") = 'ca' AND NOT ca."Disabled"
),
applicable AS (
    SELECT * FROM matched
    WHERE catalan_title IS NULL OR catalan_title = standard_catalan_title
),
updated AS (
    UPDATE public."MenuItemTranslations" t
    SET "Title" = x.title, "Disabled" = false, "UpdatedOn" = NOW()
    FROM applicable x
    WHERE t."MenuItemId" = x.menu_item_id
      AND lower(t."LanguageCode") = x.language_code
      AND (t."Title" IS DISTINCT FROM x.title OR t."Disabled")
      AND (t."Disabled" OR btrim(t."Title") = '' OR t."Title" = x.catalan_title)
    RETURNING 1
),
inserted AS (
    INSERT INTO public."MenuItemTranslations"
        ("Id", "MenuItemId", "LanguageCode", "Title", "CreatedOn", "UpdatedOn", "Disabled")
    SELECT gen_random_uuid(), x.menu_item_id, x.language_code, x.title, NOW(), NOW(), false
    FROM applicable x
    WHERE NOT EXISTS (
        SELECT 1 FROM public."MenuItemTranslations" t
        WHERE t."MenuItemId" = x.menu_item_id AND lower(t."LanguageCode") = x.language_code
    )
    RETURNING 1
)
SELECT
    (SELECT count(*) FROM updated) AS updated_translations,
    (SELECT count(*) FROM inserted) AS inserted_translations,
    (SELECT count(DISTINCT menu_key) FROM matched
     WHERE catalan_title IS DISTINCT FROM standard_catalan_title AND catalan_title IS NOT NULL) AS skipped_renamed_items,
    (SELECT count(DISTINCT s.menu_key) FROM source s
     WHERE NOT EXISTS (SELECT 1 FROM public."MenuItems" m WHERE m."Key" = s.menu_key)) AS keys_not_in_this_environment;

-- 2. Review: menu items whose es/en title still equals the Catalan one.
--    On the standard menu only titles that are the same word in each language
--    remain (General, Verifactu, Informes, Empresa, ...). Anything else is an
--    item this script skipped or does not know: translate it by hand.
SELECT m."Key", ca."Title" AS catalan_title, t."LanguageCode", t."Title"
FROM public."MenuItems" m
JOIN public."MenuItemTranslations" t ON t."MenuItemId" = m."Id" AND NOT t."Disabled"
JOIN public."MenuItemTranslations" ca
    ON ca."MenuItemId" = m."Id" AND lower(ca."LanguageCode") = 'ca' AND NOT ca."Disabled"
WHERE lower(t."LanguageCode") IN ('es', 'en')
  AND t."Title" = ca."Title"
ORDER BY m."SortOrder", m."Key", t."LanguageCode";
