import BaseService from "@/api/base.service";
import {
  UserMenuResponse,
  ProfileDetail,
  CreateProfileRequest,
  UpdateProfileRequest,
  ProfileMenuAssignmentRequest,
} from "@/types/profile";
import { Profile } from "@/types";

export class ProfileService extends BaseService<any> {
  constructor() {
    super("Profile");
  }

  async GetUserMenu(userId: string): Promise<UserMenuResponse | undefined> {
    try {
      const response = await this.apiClient.get(
        `${this.resource}/user/${userId}/menu`,
      );
      if (response.status === 200) return response.data as UserMenuResponse;
    } catch (err) {
      // Silent for now; upstream can fallback
    }
  }

  async GetAll(): Promise<Profile[] | undefined> {
    try {
      const response = await this.apiClient.get(this.resource);
      if (response.status === 200) return response.data as Profile[];
    } catch (err) {
      // ignore
    }
  }

  async Get(id: string): Promise<ProfileDetail | undefined> {
    try {
      const response = await this.apiClient.get(`${this.resource}/${id}`);
      if (response.status === 200) return response.data as ProfileDetail;
    } catch {}
  }

  async Create(
    payload: CreateProfileRequest,
  ): Promise<ProfileDetail | undefined> {
    try {
      const response = await this.apiClient.post(this.resource, payload);
      if (response.status === 201) return response.data as ProfileDetail;
    } catch {}
  }

  async Update(id: string, payload: UpdateProfileRequest): Promise<boolean> {
    try {
      const response = await this.apiClient.put(
        `${this.resource}/${id}`,
        payload,
      );
      return response.status === 200;
    } catch {
      return false;
    }
  }

  async Delete(id: string): Promise<boolean> {
    try {
      const response = await this.apiClient.delete(`${this.resource}/${id}`);
      return response.status === 204;
    } catch {
      return false;
    }
  }

  async GetMenuAssignment(
    profileId: string,
  ): Promise<ProfileMenuAssignmentRequest | undefined> {
    try {
      const response = await this.apiClient.get(
        `${this.resource}/${profileId}/menu`,
      );
      if (response.status === 200)
        return response.data as ProfileMenuAssignmentRequest;
      console.warn(`GetMenuAssignment returned status ${response.status}`);
    } catch (err) {
      console.error("GetMenuAssignment failed:", err);
      throw err; // Re-throw so caller knows it failed
    }
  }

  async UpdateMenuAssignment(
    profileId: string,
    payload: ProfileMenuAssignmentRequest,
  ): Promise<boolean> {
    try {
      const response = await this.apiClient.post(
        `${this.resource}/${profileId}/menu`,
        payload,
      );
      return response.status === 200 || response.status === 204;
    } catch {
      return false;
    }
  }

  async SetUserProfile(userId: string, profileId: string): Promise<boolean> {
    try {
      const response = await this.apiClient.post(
        `${this.resource}/user/${userId}/profile`,
        { profileId },
      );
      return response.status === 204;
    } catch (err) {
      return false;
    }
  }
}

export const AppProfileService = new ProfileService();
