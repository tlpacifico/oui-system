export interface Permission {
  externalId: string;
  name: string;
  category: string;
  description: string | null;
}

export interface PermissionsByCategory {
  [category: string]: Permission[];
}
