import apiClient from "@/api/api.client";
import type {
  CreateMenuItemRequest,
  MenuItemFlat,
  MenuItemNode,
  UpdateMenuItemRequest,
} from "@/types/menuitem";

const baseUrl = "/MenuItem";

export const getMenuItems = async (): Promise<MenuItemFlat[]> => {
  const { data } = await apiClient.get<MenuItemFlat[]>(baseUrl);
  return data;
};

export const getMenuItemsHierarchy = async (): Promise<MenuItemNode[]> => {
  const { data } = await apiClient.get<MenuItemNode[]>(
    `${baseUrl}?hierarchy=true`
  );
  return data;
};

export const getMenuItem = async (
  id: string
): Promise<MenuItemFlat | null> => {
  const response = await apiClient.get<MenuItemFlat>(`${baseUrl}/${id}`);
  if (response.status === 404) return null;
  return response.data;
};

export const createMenuItem = async (
  payload: CreateMenuItemRequest
): Promise<MenuItemFlat> => {
  const { data } = await apiClient.post<MenuItemFlat>(baseUrl, payload);
  return data;
};

export const updateMenuItem = async (
  id: string,
  payload: UpdateMenuItemRequest
): Promise<MenuItemFlat> => {
  const { data } = await apiClient.put<MenuItemFlat>(
    `${baseUrl}/${id}`,
    payload
  );
  return data;
};

export const deleteMenuItem = async (id: string): Promise<void> => {
  await apiClient.delete(`${baseUrl}/${id}`);
};
