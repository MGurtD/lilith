import { defineStore } from "pinia";
import { Shift, ShiftDetail } from "../types";
import Services from "../services";
import { ref } from "vue";

export const useShiftStore = defineStore("shift", {
  state: () => ({
    shift: ref(undefined as Shift | undefined),
    shifts: ref(undefined as Array<Shift> | undefined),
    shiftdetail: ref(undefined as ShiftDetail | undefined),
    shiftdetails: ref(undefined as Array<ShiftDetail> | undefined),
  }),
  getters: {},
  actions: {
    setNewShift(id: string) {
      this.shift = {
        id,
        name: "",
        disabled: false,
        details: [],
      } as Shift;
    },
    setNewShiftDetail(id: string) {
      this.shiftdetail = {
        id,
        startTime: "00:00:00",
        endTime: "23:59:59",
        isProductiveTime: true,
        shiftId: "",
      } as ShiftDetail;
    },
    //shifts
    async fetchAllShifts() {
      this.shifts = await Services.ShiftService.getAll();
    },
    async createShift(model: Shift) {
      const result = await Services.ShiftService.create(model);
      if (result) await this.fetchAllShifts();
      return result;
    },
    async updateShift(model: Shift) {
      const result = await Services.ShiftService.update(model.id, model);
      if (result) await this.fetchAllShifts();
      return result;
    },
    async deleteShift(id: string) {
      const result = await Services.ShiftService.delete(id);
      if (result) await this.fetchAllShifts();
      return result;
    },

    //shiftdetails
    async fetchShiftDetailsByShiftId(id: string) {
      this.shiftdetails = await Services.ShiftService.getDetailsByshiftId(id);
    },
    async createDetail(model: ShiftDetail) {
      const result = await Services.ShiftService.createDetail(model);
      if (result) await this.fetchAllShifts();
      return result;
    },
    async updateDetail(model: ShiftDetail) {
      const result = await Services.ShiftService.updateDetail(model);
      if (result) await this.fetchAllShifts();
      return result;
    },
    async deleteDetail(id: string) {
      const result = await Services.ShiftService.deleteDetail(id);
      if (result) await this.fetchAllShifts();
      return result;
    },
  },
});
