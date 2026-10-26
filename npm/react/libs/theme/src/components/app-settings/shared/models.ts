export type Value = string | number | boolean;

export type SettingField = {
  key: string;
  label: string;
  type: 'text' | 'email' | 'number' | 'checkbox' | 'select';
  options?: { value: string; label: string }[];
  initial: Value;
  min?: number;
  max?: number;
  required?: boolean;
};

export type FormDefinition = { id: string; fields: SettingField[] };

export type DraftState = {
  saved: Record<string, Record<string, Value>>;
};
