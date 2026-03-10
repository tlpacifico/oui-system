export interface Permission {
  externalId: string;
  name: string;
  category: string;
  description: string | null;
}

export interface PermissionsByCategory {
  [category: string]: Permission[];
}

export interface CreatePermissionRequest {
  name: string;
  description: string | null;
}

export interface UpdatePermissionRequest {
  name: string;
  description: string | null;
}
