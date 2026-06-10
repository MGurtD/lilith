import apiClient from "../../../api/api.client";

interface GenericResponse {
  result: boolean;
  errors: string[];
  content: unknown;
}

export default class SupportService {
  private readonly resource = "github/draft-issues";

  async createRequest(resum: string, descripcio: string): Promise<string> {
    const response = await apiClient.post(this.resource, { resum, descripcio });

    if (response.status === 200 || response.status === 201) {
      return (response.data as { id: string })?.id ?? "";
    }

    const data = response.data as GenericResponse;
    const message = data?.errors?.length
      ? data.errors.join(". ")
      : "Error en registrar la sol·licitud de suport";
    throw new Error(message);
  }
}
