export interface Role {
  externalId: string;
  name: string;
  description: string | null;
  isSystemRole: boolean;
  userCount: number;
  permissionCount: number;
  createdOn: string;
  createdBy: string | null;
  updatedOn: string | null;
  updatedBy: string | null;
}

export interface RoleDetail extends Role {
  permissions: Permission[];
}

export interface Permission {
  externalId: string;
  name: string;
  category: string;
  description: string | null;
}

export interface CreateRoleRequest {
  name: string;
  description: string | null;
}

export interface UpdateRoleRequest {
  name: string;
  description: string | null;
}
