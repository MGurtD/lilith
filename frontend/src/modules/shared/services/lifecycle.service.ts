import BaseService from "../../../api/base.service";
import {
  Lifecycle,
  Status,
  StatusTransition,
  LifecycleTag,
  AvailableStatusTransitionDto,
} from "../types";

export default class LifecycleService extends BaseService<Lifecycle> {
  async getByName(name: string): Promise<Lifecycle | undefined> {
    const response = await this.apiClient.get(`${this.resource}/name/${name}`);
    if (response.status === 200) {
      return response.data as Lifecycle;
    }
  }
  // Statuses
  async createStatus(model: Status): Promise<boolean> {
    const response = await this.apiClient.post(
      `${this.resource}/Status`,
      model
    );
    return response.status === 200 || response.status === 201;
  }
  async updateStatus(model: Status): Promise<boolean> {
    const response = await this.apiClient.put(
      `${this.resource}/Status/${model.id}`,
      model
    );
    return response.status === 200 || response.status === 201;
  }
  async deleteStatus(id: string): Promise<boolean> {
    const response = await this.apiClient.delete(
      `${this.resource}/Status/${id}`
    );
    return response.status === 200 || response.status === 201;
  }
  // Transitions
  async createTransition(model: StatusTransition): Promise<boolean> {
    const response = await this.apiClient.post(
      `${this.resource}/StatusTransition`,
      model
    );
    return response.status === 200 || response.status === 201;
  }
  async updateTransition(model: StatusTransition): Promise<boolean> {
    const response = await this.apiClient.put(
      `${this.resource}/StatusTransition/${model.id}`,
      model
    );
    return response.status === 200 || response.status === 201;
  }
  async deleteTransition(id: string): Promise<boolean> {
    const response = await this.apiClient.delete(
      `${this.resource}/StatusTransition/${id}`
    );
    return response.status === 200 || response.status === 201;
  }

  async getAvailableTransitions(
    statusId: string
  ): Promise<AvailableStatusTransitionDto[] | undefined> {
    try {
      const response = await this.apiClient.get(
        `${this.resource}/Status/${statusId}/AvailableTransitions`
      );
      if (response.status === 200) {
        return response.data as AvailableStatusTransitionDto[];
      }
    } catch (err) {
      console.error("Error getting available transitions:", err);
    }
  }

  // Tags
  async getTagById(id: string): Promise<LifecycleTag | undefined> {
    try {
      const response = await this.apiClient.get(`${this.resource}/Tag/${id}`);
      if (response.status === 200) {
        return response.data as LifecycleTag;
      }
    } catch (err) {
      console.error("Error getting tag by ID:", err);
    }
  }

  async getTagsByLifecycle(
    lifecycleId: string
  ): Promise<LifecycleTag[] | undefined> {
    try {
      const response = await this.apiClient.get(
        `${this.resource}/Tag/Lifecycle/${lifecycleId}`
      );
      if (response.status === 200) {
        return response.data as LifecycleTag[];
      }
    } catch (err) {
      console.error("Error getting lifecycle tags:", err);
    }
  }

  async createTag(lifecycleId: string, tag: LifecycleTag): Promise<boolean> {
    try {
      const response = await this.apiClient.post(
        `${this.resource}/Tag/Lifecycle/${lifecycleId}`,
        tag
      );
      return response.status === 201;
    } catch (err) {
      console.error("Error creating lifecycle tag:", err);
      return false;
    }
  }

  async updateTag(tag: LifecycleTag): Promise<boolean> {
    try {
      const response = await this.apiClient.put(
        `${this.resource}/Tag/${tag.id}`,
        tag
      );
      return response.status === 200;
    } catch (err) {
      console.error("Error updating lifecycle tag:", err);
      return false;
    }
  }

  async deleteTag(id: string): Promise<boolean> {
    try {
      const response = await this.apiClient.delete(
        `${this.resource}/Tag/${id}`
      );
      return response.status === 200;
    } catch (err) {
      console.error("Error deleting lifecycle tag:", err);
      return false;
    }
  }

  async getTagsByStatus(statusId: string): Promise<LifecycleTag[] | undefined> {
    try {
      const response = await this.apiClient.get(
        `${this.resource}/Tag/Status/${statusId}`
      );
      if (response.status === 200) {
        return response.data as LifecycleTag[];
      }
    } catch (err) {
      console.error("Error getting tags by status:", err);
    }
  }

  async assignTagToStatus(statusId: string, tagId: string): Promise<boolean> {
    try {
      const response = await this.apiClient.post(
        `${this.resource}/Status/${statusId}/Tag/${tagId}`
      );
      return response.status === 200;
    } catch (err) {
      console.error("Error assigning tag to status:", err);
      return false;
    }
  }

  async removeTagFromStatus(statusId: string, tagId: string): Promise<boolean> {
    try {
      const response = await this.apiClient.delete(
        `${this.resource}/Status/${statusId}/Tag/${tagId}`
      );
      return response.status === 200;
    } catch (err) {
      console.error("Error removing tag from status:", err);
      return false;
    }
  }
}
