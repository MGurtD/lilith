export interface GenericResponse<T> {
  result: boolean;
  errors: Array<string>;
  content?: T | null;
}

export interface AuthenticationResponse {
  token: string;
  refreshToken: string;
  result: boolean;
  errors: Array<string>;
}

export interface Region {
  id: string;
  nm: string;
}

export interface Role {
  id: string;
  name: string;
}

export interface User {
  id: string;
  username: string;
  firstName: string;
  lastName: string;
  disabled: boolean;
  preferredLanguage: string;
  roleId: string;
  role?: Role;
  profileId?: string | null;
  profile?: Profile;
}

export interface UserFilter {
  id: string;
  userId: string;
  page: string;
  key: string;
  filter: string;
}

export interface Language {
  id: string;
  code: string;
  name: string;
  icon?: string;
  isDefault: boolean;
  sortOrder?: number;
}

export interface Profile {
  id: string;
  name: string;
  description?: string;
  isSystem?: boolean;
}
export interface ApiKey {
  id: string;
  name: string;
  description?: string;
  keyPrefix: string;
  scopes?: string;
  expiresOn?: string | null;
  lastUsedOn?: string | null;
  disabled: boolean;
  createdOn: string;
  updatedOn: string;
}

export interface CreateApiKeyResponse {
  id: string;
  name: string;
  keyPrefix: string;
  apiKey: string;
  expiresOn?: string | null;
}

export interface File {
  entity: string;
  entityId: string;
  type: number;
  size: number;
  originalName: string;
  path: string;
  id: string;
  createdOn: string;
  updatedOn: string;
  disabled: boolean;
}
