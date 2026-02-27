export interface SystemSetting {
  key: string;
  value: string;
  valueType: string;
  module: string;
  displayName: string;
  description: string | null;
}

export interface SystemSettingGroup {
  module: string;
  settings: SystemSetting[];
}

export interface UpdateSystemSettingRequest {
  value: string;
}
