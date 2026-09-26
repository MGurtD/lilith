export interface MenuItemFlat {
  id: string;
  key: string;
  title: string;
  icon?: string | null;
  route?: string | null;
  parentId?: string | null;
  sortOrder: number;
  disabled?: boolean;
  translations: MenuItemTranslation[];
}

export interface MenuItemTranslation {
  languageCode: string;
  title: string;
}

export interface MenuItemNode extends MenuItemFlat {
  children?: MenuItemNode[];
}

export interface CreateMenuItemRequest {
  id: string;
  key: string;
  icon?: string | null;
  route?: string | null;
  parentId?: string | null;
  sortOrder: number;
  translations: MenuItemTranslation[];
}

export interface UpdateMenuItemRequest extends CreateMenuItemRequest {
  id: string;
}

export interface MenuItemTranslationMatrixLanguage {
  id: string;
  code: string;
  name: string;
  icon?: string;
  isDefault: boolean;
  sortOrder: number;
}

export interface MenuItemTranslationMatrixRow {
  id: string;
  key: string;
  route?: string | null;
  parentId?: string | null;
  sortOrder: number;
  disabled: boolean;
  depth: number;
  translations: MenuItemTranslation[];
}

export interface MenuItemTranslationMatrix {
  languages: MenuItemTranslationMatrixLanguage[];
  items: MenuItemTranslationMatrixRow[];
}

export interface UpdateMenuItemTranslationRowRequest {
  menuItemId: string;
  translations: MenuItemTranslation[];
}

export interface UpdateMenuItemTranslationsRequest {
  items: UpdateMenuItemTranslationRowRequest[];
}

export interface UpdateMenuItemTranslationsResult {
  updatedMenuItems: number;
  updatedTranslations: number;
}

export interface MenuItemImportResult {
  createdItems: number;
  updatedItems: number;
  updatedTranslations: number;
}
