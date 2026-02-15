import { defineStore } from "pinia";
import { AppProfileService } from "../services/profile.service";
import type { Profile } from "../types";
import type {
  ProfileDetail,
  ProfileMenuAssignmentRequest,
  CreateProfileRequest,
  UpdateProfileRequest,
} from "../types/profile";

interface State {
  items: Profile[];
  current: ProfileDetail | null;
  menuAssignment: ProfileMenuAssignmentRequest | null;
  loading: boolean;
  saving: boolean;
  menuLoading: boolean;
}

export const useProfilesStore = defineStore("profiles", {
  state: (): State => ({
    items: [],
    current: null,
    menuAssignment: null,
    loading: false,
    saving: false,
    menuLoading: false,
  }),
  actions: {
    async fetchAll() {
      this.loading = true;
      try {
        const data = await AppProfileService.GetAll();
        this.items = data ?? [];
      } finally {
        this.loading = false;
      }
    },
    async fetchOne(id: string) {
      this.loading = true;
      try {
        this.current = (await AppProfileService.Get(id)) ?? null;
        if (!this.current) {
          this.menuAssignment = null;
        }
      } finally {
        this.loading = false;
      }
    },
    setNew(id?: string) {
      this.current = {
        id: id || "",
        name: "",
        description: "",
        isSystem: false,
        menuItemIds: [],
        defaultMenuItemId: null,
      };
      this.menuAssignment = { menuItemIds: [], defaultMenuItemId: null };
    },
    async create(payload: CreateProfileRequest) {
      this.saving = true;
      try {
        const created = await AppProfileService.Create(payload);
        if (created) {
          this.current = created;
          await this.fetchAll();
          return true;
        }
        return false;
      } finally {
        this.saving = false;
      }
    },
    async update(id: string, payload: UpdateProfileRequest) {
      this.saving = true;
      try {
        const ok = await AppProfileService.Update(id, payload);
        if (ok) {
          await this.fetchAll();
        }
        return ok;
      } finally {
        this.saving = false;
      }
    },
    async remove(id: string) {
      this.saving = true;
      try {
        const ok = await AppProfileService.Delete(id);
        if (ok) await this.fetchAll();
        return ok;
      } finally {
        this.saving = false;
      }
    },
    async fetchMenuAssignment(profileId: string) {
      this.menuLoading = true;
      try {
        this.menuAssignment = (await AppProfileService.GetMenuAssignment(
          profileId,
        )) ?? { menuItemIds: [], defaultMenuItemId: null };
      } finally {
        this.menuLoading = false;
      }
    },
    setMenuSelection(ids: string[]) {
      if (!this.menuAssignment)
        this.menuAssignment = { menuItemIds: [], defaultMenuItemId: null };
      this.menuAssignment.menuItemIds = ids;
      if (
        this.menuAssignment.defaultMenuItemId &&
        !ids.includes(this.menuAssignment.defaultMenuItemId)
      ) {
        this.menuAssignment.defaultMenuItemId = null;
      }
    },
    setDefaultMenu(id: string | null) {
      if (!this.menuAssignment) return;
      this.menuAssignment.defaultMenuItemId = id;
    },
    async saveMenuAssignment() {
      if (!this.current || !this.menuAssignment) return false;
      this.menuLoading = true;
      try {
        return await AppProfileService.UpdateMenuAssignment(
          this.current.id,
          this.menuAssignment,
        );
      } finally {
        this.menuLoading = false;
      }
    },
  },
});
