using Application.Contracts;
using Application.Services;
using Domain.Entities.Auth;

namespace Application.Services.System;

public class MenuItemService(IUnitOfWork unitOfWork, ILocalizationService localization) : IMenuItemService
{
    public async Task<GenericResponse> GetAll(bool hierarchy = false)
    {
        var items = (await unitOfWork.MenuItems.GetAll()).OrderBy(i => i.SortOrder).ToList();
        if (!hierarchy)
            return new GenericResponse(true, items);

        var dict = items.ToDictionary(i => i.Id, i => new Node(i));
        var roots = new List<Node>();
        foreach (var n in dict.Values)
        {
            if (n.ParentId.HasValue && dict.ContainsKey(n.ParentId.Value))
                dict[n.ParentId.Value].Children.Add(n);
            else roots.Add(n);
        }
        return new GenericResponse(true, roots);
    }

    public async Task<GenericResponse> Get(Guid id)
    {
        var item = await unitOfWork.MenuItems.Get(id);
        if (item is null)
            return new GenericResponse(false, localization.GetLocalizedString("MenuItemNotFound", id));
        return new GenericResponse(true, item);
    }

    public async Task<GenericResponse> Create(MenuItem item)
    {
        if (unitOfWork.MenuItems.Find(m => m.Key == item.Key).Any())
            return new GenericResponse(false, localization.GetLocalizedString("MenuItemKeyExists", item.Key));
        if (item.ParentId.HasValue)
        {
            var parent = await unitOfWork.MenuItems.Get(item.ParentId.Value);
            if (parent is null)
                return new GenericResponse(false, localization.GetLocalizedString("MenuItemNotFound", item.ParentId.Value));
        }
        await unitOfWork.MenuItems.Add(item);
        return new GenericResponse(true, item);
    }

    public async Task<GenericResponse> Update(MenuItem item)
    {
        var current = await unitOfWork.MenuItems.Get(item.Id);
        if (current is null)
            return new GenericResponse(false, localization.GetLocalizedString("MenuItemNotFound", item.Id));
        if (current.Key != item.Key && unitOfWork.MenuItems.Find(m => m.Key == item.Key).Any())
            return new GenericResponse(false, localization.GetLocalizedString("MenuItemKeyExists", item.Key));
        if (item.ParentId.HasValue)
        {
            if (item.ParentId.Value == item.Id)
                return new GenericResponse(false, localization.GetLocalizedString("InvalidMenuHierarchy"));
            var parent = await unitOfWork.MenuItems.Get(item.ParentId.Value);
            if (parent is null)
                return new GenericResponse(false, localization.GetLocalizedString("MenuItemNotFound", item.ParentId.Value));
            // Detect cycle: walk up parents
            var walker = parent;
            while (walker != null)
            {
                if (walker.Id == item.Id)
                    return new GenericResponse(false, localization.GetLocalizedString("InvalidMenuHierarchy"));
                walker = walker.ParentId.HasValue ? await unitOfWork.MenuItems.Get(walker.ParentId.Value) : null;
            }
        }
        current.Title = item.Title;
        current.Icon = item.Icon;
        current.Route = item.Route;
        current.ParentId = item.ParentId;
        current.SortOrder = item.SortOrder;
        await unitOfWork.MenuItems.Update(current);
        return new GenericResponse(true, current);
    }

    public async Task<GenericResponse> Delete(Guid id)
    {
        var item = await unitOfWork.MenuItems.Get(id);
        if (item is null)
            return new GenericResponse(false, localization.GetLocalizedString("MenuItemNotFound", id));
        var hasChildren = unitOfWork.MenuItems.Find(m => m.ParentId == id).Any();
        if (hasChildren)
            return new GenericResponse(false, localization.GetLocalizedString("InvalidMenuHierarchy")); // reuse or add specific key
        await unitOfWork.MenuItems.Remove(item);
        return new GenericResponse(true, true);
    }

    private record Node
    {
        public Guid Id { get; init; }
        public string Key { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string? Icon { get; init; }
        public string? Route { get; init; }
        public int SortOrder { get; init; }
        public Guid? ParentId { get; init; }
        public List<Node> Children { get; } = new();
        public Node(MenuItem item)
        {
            Id = item.Id; Key = item.Key; Title = item.Title; Icon = item.Icon; Route = item.Route; SortOrder = item.SortOrder; ParentId = item.ParentId;
        }
    }
}




