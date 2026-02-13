export interface UserRole {
  roleId: string;
  roleName: string;
  roleDescription: string | null;
  assignedOn: string;
  assignedBy: string | null;
}

export interface AssignRoleRequest {
  roleExternalId: string;
}

export interface AssignBulkRolesRequest {
  roleExternalIds: string[];
}
