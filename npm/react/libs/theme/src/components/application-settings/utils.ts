import type { SettingField } from './models';

const toggle = (key: string, label: string, initial = false): SettingField => ({
  key,
  label,
  type: 'checkbox',
  initial,
});

const number = (
  key: string,
  label: string,
  initial: number,
  min = 1,
  max = 10080,
): SettingField => ({
  key,
  label,
  type: 'number',
  initial,
  min,
  max,
  required: true,
});

const text = (
  key: string,
  label: string,
  initial = '',
  required = false,
): SettingField => ({ key, label, type: 'text', initial, required });

export { toggle, number, text };
