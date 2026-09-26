import apiClient from "@/api/api.client";
import type {
  CreateMenuItemRequest,
  MenuItemImportResult,
  MenuItemFlat,
  MenuItemNode,
  MenuItemTranslationMatrix,
  UpdateMenuItemTranslationsRequest,
  UpdateMenuItemTranslationsResult,
  UpdateMenuItemRequest,
} from "@/modules/system/types/menuitem";

const baseUrl = "/MenuItem";

interface TransferErrorResponse {
  errors?: string[];
}

const transferError = (data: TransferErrorResponse | undefined) =>
  new Error(data?.errors?.[0] ?? "");

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

export const getMenuItemTranslationMatrix = async (): Promise<MenuItemTranslationMatrix> => {
  const { data } = await apiClient.get<MenuItemTranslationMatrix>(
    `${baseUrl}/translations`,
  );
  return data;
};

export const updateMenuItemTranslations = async (
  payload: UpdateMenuItemTranslationsRequest,
): Promise<UpdateMenuItemTranslationsResult> => {
  const { data } = await apiClient.patch<UpdateMenuItemTranslationsResult>(
    `${baseUrl}/translations`,
    payload,
  );
  return data;
};

export const exportMenuItems = async (): Promise<{
  blob: Blob;
  fileName: string;
}> => {
  const response = await apiClient.get<Blob>(`${baseUrl}/export`, {
    responseType: "blob",
  });
  if (response.status !== 200) {
    let error: TransferErrorResponse | undefined;
    try {
      error = JSON.parse(await response.data.text()) as TransferErrorResponse;
    } catch {
      error = undefined;
    }
    throw transferError(error);
  }

  const disposition = response.headers["content-disposition"] as
    | string
    | undefined;
  const encodedName = disposition?.match(/filename\*=UTF-8''([^;]+)/i)?.[1];
  const plainName = disposition?.match(/filename="?([^";]+)"?/i)?.[1];
  const fallbackDate = new Date().toISOString().slice(0, 10).replaceAll("-", "");

  return {
    blob: response.data,
    fileName: encodedName
      ? decodeURIComponent(encodedName)
      : plainName ?? `menu-items-${fallbackDate}.json`,
  };
};

export const importMenuItems = async (
  file: File,
): Promise<MenuItemImportResult> => {
  const formData = new FormData();
  formData.append("file", file);
  const response = await apiClient.post<
    MenuItemImportResult | TransferErrorResponse
  >(`${baseUrl}/import`, formData, {
    headers: { "Content-Type": "multipart/form-data" },
  });
  if (response.status !== 200) {
    throw transferError(response.data as TransferErrorResponse);
  }
  return response.data as MenuItemImportResult;
};
