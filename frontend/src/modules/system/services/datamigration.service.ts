import apiClient, { logException } from "@/api/api.client";

export interface MigrationEntityInfo {
  key: string;
  displayNameKey: string;
}

export interface ImportRowError {
  sheet: string;
  row: number;
  code?: string | null;
  reason: string;
}

export interface ImportReport {
  total: number;
  inserted: number;
  skipped: number;
  errors: ImportRowError[];
}

const fileRequestTimeout =
  (import.meta.env.VITE_API_FILE_REQUEST_TIMEOUT as number) ?? 60000;

export class DataMigrationService {
  private resource = "/DataMigration";

  async getEntities(): Promise<MigrationEntityInfo[]> {
    try {
      const response = await apiClient.get(`${this.resource}/Entities`);
      if (response.status === 200) return response.data as MigrationEntityInfo[];
    } catch (err) {
      logException(err);
    }
    return [];
  }

  async downloadTemplate(keys: string[]): Promise<Blob | undefined> {
    return this.downloadBlob(`${this.resource}/Template`, keys);
  }

  async exportData(keys: string[]): Promise<Blob | undefined> {
    return this.downloadBlob(`${this.resource}/Export`, keys);
  }

  async import(file: File, keys: string[]): Promise<ImportReport | undefined> {
    const formData = new FormData();
    formData.append("file", file);
    keys.forEach((key) => formData.append("keys", key));

    try {
      const response = await apiClient.post(`${this.resource}/Import`, formData, {
        headers: { "Content-Type": "multipart/form-data" },
        timeout: fileRequestTimeout,
      });
      if (response.status === 200) return response.data as ImportReport;
    } catch (err) {
      logException(err);
    }
    return undefined;
  }

  private async downloadBlob(
    path: string,
    keys: string[],
  ): Promise<Blob | undefined> {
    const query = keys
      .map((key) => `keys=${encodeURIComponent(key)}`)
      .join("&");

    try {
      const response = await apiClient.get(`${path}?${query}`, {
        responseType: "blob",
        timeout: fileRequestTimeout,
      });
      if (response.status === 200) return response.data as Blob;
    } catch (err) {
      logException(err);
    }
    return undefined;
  }
}
