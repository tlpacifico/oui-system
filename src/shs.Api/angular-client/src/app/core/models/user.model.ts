export interface User {
  externalId: string;
  email: string;
  displayName: string | null;
  createdOn: string;
  roles: { externalId: string; name: string }[];
  roleCount: number;
}

export interface UserDetail {
  externalId: string;
  email: string;
  displayName: string | null;
  createdOn: string;
  roles: { externalId: string; name: string; assignedOn: string; assignedBy: string }[];
  roleCount: number;
}

export interface CreateUserRequest {
  email: string;
  password: string;
  displayName: string | null;
}

export interface UpdateUserRequest {
  displayName: string | null;
}
