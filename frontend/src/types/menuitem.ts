export interface MenuItemFlat {
  id: string;
  key: string;
  title: string;
  icon?: string | null;
  route?: string | null;
  parentId?: string | null;
  sortOrder: number;
  disabled?: boolean;
}

export interface MenuItemNode extends MenuItemFlat {
  children?: MenuItemNode[];
}

export interface CreateMenuItemRequest {
  id: string;
  key: string;
  title: string;
  icon?: string | null;
  route?: string | null;
  parentId?: string | null;
  sortOrder: number;
}

export interface UpdateMenuItemRequest extends CreateMenuItemRequest {
  id: string;
}
