import { defineStore } from "pinia";
import Services from "../../../services";
import { File } from "../../../types";
import { Parameter } from "../types";
import { createBlobAndDownloadFile } from "../../../utils/functions";
import { REPORTS_ENTITY_ID } from "../../../services/report.service";

export const useReportsStore = defineStore({
  id: "reports",
  state: () => ({
    reports: undefined as Array<File> | undefined,
  }),
  getters: {
    getReportByName: (state) => {
      return (name: string) => {
        if (state.reports)
          return state.reports.find((file: File) =>
            file.originalName.includes(name)
          );
      };
    },
  },
  actions: {
    async fetchAll() {
      this.reports = await Services.File.GetEntityFiles(
        "Reports",
        REPORTS_ENTITY_ID
      );
    },
    async generateAndDownload(
      file: File,
      parameters: Array<Parameter>,
      fileName: string
    ) {
      const report = await Services.File.DownloadReport(file, parameters);
      if (report) createBlobAndDownloadFile(fileName, report);
    },
  },
});
