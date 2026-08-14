export interface MenuNode {
  id: string;
  key: string;
  title: string;
  icon?: string | null;
  route?: string | null;
  sortOrder: number;
  parentId?: string | null;
  children: MenuNode[];
}

export interface UserMenuResponse {
  items: MenuNode[];
  defaultScreen?: string;
}

export interface ProfileDetail {
  id: string;
  name: string;
  description?: string;
  isSystem?: boolean;
  menuItemIds?: string[];
  defaultMenuItemId?: string | null;
}

export interface CreateProfileRequest {
  name: string;
  description?: string;
}

export interface UpdateProfileRequest extends CreateProfileRequest {
  id: string;
}

export interface ProfileMenuAssignmentRequest {
  menuItemIds: string[];
  defaultMenuItemId?: string | null;
}
