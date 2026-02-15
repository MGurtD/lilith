using Application.Contracts;
using Domain.Entities.Auth;

namespace Api.Setup
{
    public static class ProfileMenuSeeder
    {
    private const string DEFAULT_PROFILE_KEY = "default";
    private const string MANAGEMENT_PROFILE_KEY = "management";
    private const string SHOOPFLOOR_PROFILE_KEY = "shoopfloor";
    private const string SUPERUSER_PROFILE_KEY = "superuser";


        private record SeedMenu(string Key, string Title, string? Icon, string? Route, SeedMenu[]? Children = null);

        // Full hierarchy extracted from frontend menus.ts (applicationMenus, managmentMenus, shoopflorMenus)
        private static readonly SeedMenu[] RootMenus =
        [
            // Application Menus
            new SeedMenu("general","General","pi pi-cog",null, [
                new SeedMenu("users","Usuaris","pi pi-users","/users"),
                new SeedMenu("profiles","Perfils d'usuari","pi pi-users","/profiles"),
                new SeedMenu("menuitems","Elements del menú","pi pi-sitemap","/menuitems"),
                new SeedMenu("exercise","Exercicis","pi pi-calendar","/exercise"),
                new SeedMenu("taxes","Impostos","pi pi-percentage","/taxes"),
                new SeedMenu("paymentmethods","Formes de pagament","pi pi-paypal","/payment-methods"),
                new SeedMenu("invoiceseries","Sèries de factures","pi pi-sort-numeric-down","/purchaseinvoiceserie"),
                new SeedMenu("lifecycle","Cicles de vida","pi pi-refresh","/lifecycle")
            ]),
            new SeedMenu("purchase","Compres","pi pi-cart-plus",null,[
                new SeedMenu("suppliers","Proveïdors","pi pi-bookmark","/suppliers"),
                new SeedMenu("referencetype","Tipus de materials","pi pi-palette","/referencetype"),
                new SeedMenu("material","Referències","pi pi-ticket","/material"),
                new SeedMenu("purchase_orders_group","Comandes","pi pi-shopping-bag",null,[
                    new SeedMenu("purchase_orders","Comandes","pi pi-shopping-bag","/purchase-orders"),
                    new SeedMenu("phase_to_purchase_order","Comandes des de producció","pi pi-shopping-cart","/phase-to-purchase-order")
                ]),
                new SeedMenu("receipts","Albarans","pi pi-truck","/receipts"),
                new SeedMenu("purchaseinvoice","Factures","pi pi-money-bill","/purchaseinvoice"),
                new SeedMenu("expenses_group","Despeses","pi pi-wallet",null,[
                    new SeedMenu("expensetype","Tipus de despesa","pi pi-tag","/expensetype"),
                    new SeedMenu("expense","Declaració despeses","pi pi-wallet","/expense")
                ])
            ]),
            new SeedMenu("sales","Ventes","pi pi-money-bill",null,[
                new SeedMenu("customers","Clients","pi pi-building","/customers"),
                new SeedMenu("sales_reference","Referències","pi pi-ticket","/sales/reference"),
                new SeedMenu("budget","Pressupostos","pi pi-flag","/budget"),
                new SeedMenu("salesorder","Comandes","pi pi-flag-fill","/salesorder"),
                new SeedMenu("deliverynote","Albarans d'entrega","pi pi-truck","/deliverynote"),
                new SeedMenu("sales_invoice","Factures","pi pi-wallet","/sales-invoice")
            ]),
            new SeedMenu("production","Producció","pi pi-chart-bar",null,[
                new SeedMenu("plantmodel","Model de planta","pi pi-building",null,[
                    new SeedMenu("enterprise","Empresa","pi pi-building","/enterprise"),
                    new SeedMenu("site","Locals","pi pi-building","/site"),
                    new SeedMenu("area","Arees","pi pi-building","/area"),
                    new SeedMenu("workcentertype","Tipus de màquines","pi pi-building","/workcentertype"),
                    new SeedMenu("workcenter","Màquines","pi pi-building","/workcenter")
                ]),
                new SeedMenu("shifts","Torns","pi pi-calendar","/shifts"),
                new SeedMenu("machinestatus","Estats de màquina","pi pi-database","/machinestatus"),
                new SeedMenu("workcentercost","Costs de màquina","pi pi-euro","/workcentercost"),
                new SeedMenu("operators_group","Operaris","pi pi-users",null,[
                    new SeedMenu("operatortype","Tipus d'operari","pi pi-users","/operatortype"),
                    new SeedMenu("operator","Operaris","pi pi-user","/operator")
                ]),
                new SeedMenu("workmaster","Rutes de fabricació","pi pi-reply","/workmaster"),
                new SeedMenu("workorder","Ordres de fabricació","pi pi-book","/workorder"),
                new SeedMenu("productionpart","Tiquets de producció","pi pi-stopwatch","/productionpart"),
                new SeedMenu("workcentershift","Històric de producció","pi pi-calendar","/workcentershift")
            ]),
            new SeedMenu("warehouse","Magatzem","pi pi-box",null,[
                new SeedMenu("warehouse_list","Magatzems","pi pi-box","/warehouse"),
                new SeedMenu("stocks","Estocs","pi pi-bars","/stocks"),
                new SeedMenu("stockmovement","Moviments","pi pi-arrow-right-arrow-left","/stockmovement"),
                new SeedMenu("inventory","Inventari","pi pi-sort-alt","/inventory")
            ]),
            new SeedMenu("statistics","Estadístiques","pi pi-chart-pie",null,[
                new SeedMenu("incomes_vs_expenses","Facturació vs Despeses","pi pi-wallet","/incomesandexpensesdashboard"),
                new SeedMenu("expense_dashboard","Despeses","pi pi-wallet","/expense-dashboard"),
                new SeedMenu("productioncost","Costs Producció","pi pi-euro","/productioncost"),
                new SeedMenu("ranking_customers","Ranking Clients","pi pi-euro","/ranking-customers")
            ]),
            new SeedMenu("verifactu","Verifactu","pi pi-shield",null,[
                new SeedMenu("invoice_integration","Integració de factures","pi pi-upload","/verifactu/invoice-integration"),
                new SeedMenu("integration_requests","Peticions d'integració","pi pi-search","/verifactu/integration-requests"),
                new SeedMenu("find_invoices","Consulta a Verifactu","pi pi-search-plus","/verifactu/find-invoices")
            ]),
            // Management menus
            new SeedMenu("management_root","Gestió de factures","pi pi-book",null,[
                new SeedMenu("management_purchaseinvoices","Factures de compra","pi pi-download","/purchaseinvoices-by-period"),
                new SeedMenu("management_salesinvoices","Factures de venta","pi pi-upload","/salesinvoices-by-period")
            ]),
            // Plant menus
            new SeedMenu("shopfloor_root","Planta","pi pi-cog","/plant")
        ];

        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // 1. Ensure all menu items exist (idempotent)
            var existingMenuItems = (await uow.MenuItems.GetAll()).ToDictionary(m => m.Key, m => m);
            var menuLookup = new Dictionary<string, MenuItem>(existingMenuItems);
            foreach (var root in RootMenus)
            {
                await CreateMenuRecursive(root, null, menuLookup, uow);
            }

            // Refresh after potential insertions
            existingMenuItems = (await uow.MenuItems.GetAll()).ToDictionary(m => m.Key, m => m);

            // 2. Ensure required system profiles exist
            var profiles = (await uow.Profiles.GetAll()).ToDictionary(p => p.Name, p => p);
            if (!profiles.TryGetValue(DEFAULT_PROFILE_KEY, out var defaultProfile))
            {
                defaultProfile = new Profile { Name = DEFAULT_PROFILE_KEY, Description = "System default profile", IsSystem = true };
                await uow.Profiles.Add(defaultProfile);
                profiles[DEFAULT_PROFILE_KEY] = defaultProfile;
            }
            if (!profiles.TryGetValue(MANAGEMENT_PROFILE_KEY, out var managementProfile))
            {
                managementProfile = new Profile { Name = MANAGEMENT_PROFILE_KEY, Description = "Management profile", IsSystem = true };
                await uow.Profiles.Add(managementProfile);
                profiles[MANAGEMENT_PROFILE_KEY] = managementProfile;
            }
            if (!profiles.TryGetValue(SHOOPFLOOR_PROFILE_KEY, out var shopfloorProfile))
            {
                shopfloorProfile = new Profile { Name = SHOOPFLOOR_PROFILE_KEY, Description = "Shopfloor profile", IsSystem = true };
                await uow.Profiles.Add(shopfloorProfile);
                profiles[SHOOPFLOOR_PROFILE_KEY] = shopfloorProfile;
            }
            if (!profiles.TryGetValue(SUPERUSER_PROFILE_KEY, out var superuserProfile))
            {
                superuserProfile = new Profile { Name = SUPERUSER_PROFILE_KEY, Description = "Superuser profile with full access", IsSystem = true };
                await uow.Profiles.Add(superuserProfile);
                profiles[SUPERUSER_PROFILE_KEY] = superuserProfile;
            }

            // 3. Define profile menu key sets
            var managementKeys = new HashSet<string> { "management_root", "management_purchaseinvoices", "management_salesinvoices" };
            var shopfloorKeys = new HashSet<string> { "shopfloor_root" };
            var superUserOnlyKeys = new HashSet<string> { "users", "profiles", "menuitems" };

            var superuserKeys = existingMenuItems.Values.Select(v => v.Key).ToHashSet();
            // Default profile: all except management & shopfloor specific menus
            var defaultKeys = existingMenuItems.Values
                .Select(v => v.Key)
                .Where(k => !managementKeys.Contains(k) && !shopfloorKeys.Contains(k) && !superUserOnlyKeys.Contains(k))
                .ToHashSet();

            // 4. Assign menus with tailored sets
            await EnsureProfileAssignments(uow, defaultProfile, defaultKeys, existingMenuItems, defaultScreenKey: FindFirstLeaf(existingMenuItems, defaultKeys), allowRemoval: true);
            await EnsureProfileAssignments(uow, managementProfile, managementKeys, existingMenuItems, defaultScreenKey: "management_purchaseinvoices", allowRemoval: true);
            await EnsureProfileAssignments(uow, shopfloorProfile, shopfloorKeys, existingMenuItems, defaultScreenKey: "shopfloor_root", allowRemoval: true);
            await EnsureProfileAssignments(uow, superuserProfile, superuserKeys, existingMenuItems, defaultScreenKey: FindFirstLeaf(existingMenuItems, superuserKeys), allowRemoval: true);

            // 5. Assign default profile to users without one
            await AssignProfileToUsersWithoutProfile(uow, defaultProfile.Id);
        }

        private static string? FindFirstLeaf(Dictionary<string, MenuItem> items, HashSet<string>? filterKeys = null)
        {
            // Determine a leaf: item whose Id isn't a ParentId of others
            var parents = items.Values.Where(i => i.ParentId != null).Select(i => i.ParentId!.Value).ToHashSet();
            var query = items.Values.Where(i => !parents.Contains(i.Id) && !string.IsNullOrWhiteSpace(i.Route));
            if (filterKeys != null && filterKeys.Count > 0)
            {
                query = query.Where(i => filterKeys.Contains(i.Key));
            }
            var firstLeaf = query.OrderBy(i => i.SortOrder).FirstOrDefault();
            return firstLeaf?.Key;
        }

        private static async Task EnsureProfileAssignments(IUnitOfWork uow, Profile profile, HashSet<string> desiredKeys, Dictionary<string, MenuItem> allItems, string? defaultScreenKey, bool allowRemoval = false)
        {
            var currentAssignments = uow.ProfileMenuItems.Find(p => p.ProfileId == profile.Id).ToList();
            var desiredIds = desiredKeys.Select(k => allItems[k].Id).ToHashSet();

            // Add missing
            foreach (var key in desiredKeys)
            {
                var id = allItems[key].Id;
                if (!currentAssignments.Any(a => a.MenuItemId == id))
                {
                    await uow.ProfileMenuItems.Add(new ProfileMenuItem { ProfileId = profile.Id, MenuItemId = id, IsDefault = false });
                }
            }
            // Remove extras if allowed (now enabled for default to drop excluded sets)
            if (allowRemoval)
            {
                foreach (var extra in currentAssignments.Where(a => !desiredIds.Contains(a.MenuItemId)).ToList())
                {
                    await uow.ProfileMenuItems.Remove(extra);
                }
            }
            // Set default
            if (!string.IsNullOrWhiteSpace(defaultScreenKey) && allItems.TryGetValue(defaultScreenKey, out var defaultItem))
            {
                var assignments = uow.ProfileMenuItems.Find(p => p.ProfileId == profile.Id).ToList();
                foreach (var a in assignments)
                {
                    a.IsDefault = a.MenuItemId == defaultItem.Id;
                    await uow.ProfileMenuItems.Update(a);
                }
            }
        }

        private static async Task CreateMenuRecursive(SeedMenu seed, Guid? parentId, Dictionary<string, MenuItem> lookup, IUnitOfWork uow, int level = 0)
        {
            if (lookup.ContainsKey(seed.Key)) return; // avoid duplicates
            var menuItem = new MenuItem
            {
                Key = seed.Key,
                Title = seed.Title,
                Icon = seed.Icon,
                Route = seed.Route,
                ParentId = parentId,
                SortOrder = lookup.Count
            };
            await uow.MenuItems.Add(menuItem);
            lookup[seed.Key] = menuItem;

            if (seed.Children != null)
            {
                foreach (var child in seed.Children)
                {
                    await CreateMenuRecursive(child, menuItem.Id, lookup, uow, level+1);
                }
            }
        }

        private static async Task AssignProfileToUsersWithoutProfile(IUnitOfWork uow, Guid? profileId = null)
        {
            var users = await uow.Users.GetAll();
            var profiles = await uow.Profiles.GetAll();
            //Temges
            foreach (var user in users.Where(u => u.ProfileId == null))
            {
                if (user.Username == "ezapater" || user.Username == "marcgurt")
                {
                    var superuserProfile = profiles.Where(u => u.Name == "superuser").FirstOrDefault();
                    if (superuserProfile != null)
                    {
                        user.ProfileId = superuserProfile.Id;
                    }
                }
                else if (user.Username == "gestoria")
                {
                    var managementProfile = profiles.Where(u => u.Name == "management").FirstOrDefault();
                    if (managementProfile != null)
                    {
                        user.ProfileId = managementProfile.Id;
                    }
                } else {
                    var defaultProfile = profiles.Where(u => u.Name == "default").FirstOrDefault();
                    if (defaultProfile != null)
                    {
                        user.ProfileId = defaultProfile.Id;
                    }
                }
                await uow.Users.Update(user);
            }
        }
    }
}
