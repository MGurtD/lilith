import { AxiosInstance } from "axios";
import { File } from "@/types";
import apiClient from "@/api/api.client";
import { Parameter } from "@/modules/shared/types";

const fileRequestTimeout =
  (import.meta.env.VITE_API_FILE_REQUEST_TIMEOUT as number) ?? 20000;

export class FileService {
  public apiClient: AxiosInstance;
  private resource: string;
  constructor() {
    this.apiClient = apiClient;
    this.resource = "/File";
  }

  public async GetEntityFiles(
    entity: string,
    id: string,
  ): Promise<undefined | Array<File>> {
    let response = await this.apiClient.get(
      `${this.resource}?entity=${entity}&entityId=${id}`,
    );
    if (response.status === 200) {
      return response.data;
    }
  }

  public async Download(
    file: File,
  ): Promise<{ blob: Blob; contentType: string }> {
    let response = await this.apiClient.post(
      `${this.resource}/Download`,
      file,
      { responseType: "blob", timeout: fileRequestTimeout },
    );
    if (response.status === 200) {
      const contentType =
        response.headers["content-type"] || "application/octet-stream";
      return {
        blob: response.data,
        contentType: contentType,
      };
    }
    throw new Error("");
  }

  public async DownloadReport(
    file: File,
    parameters: Array<Parameter>,
  ): Promise<any> {
    let response = await this.apiClient.post(
      `Report/Download`,
      { file, parameters },
      { responseType: "blob" },
    );
    if (response.status === 200) {
      return response.data;
    }
  }

  public async Upload(file: Blob, entity: string, id: string): Promise<any> {
    const formData = new FormData();
    formData.append("entity", entity);
    formData.append("id", id);
    formData.append("file", file);

    let response = await this.apiClient.post(
      `${this.resource}/Upload`,
      formData,
      {
        headers: {
          "Content-Type": "multipart/form-data",
        },
        timeout: fileRequestTimeout,
      },
    );
    if (response.status === 200) {
      return response.data;
    }
  }

  public async Delete(id: string): Promise<boolean> {
    let response = await this.apiClient.delete(`${this.resource}/${id}`);
    return response.status === 200;
  }
}
