import { CircleOff, type LucideIcon } from 'lucide-react';

export type SegmentedOption = {
  label: string;
  value: string;
  icon?: LucideIcon;
};

export const PRESET_OPTIONS = [
  { name: 'Default', color: '#000000' },
  { name: 'Neutral', color: '#808080' },
  { name: 'Vibrant', color: '#FF0000' },
] as const;

export function getPresetColor(name: string) {
  return PRESET_OPTIONS.find((preset) => preset.name === name)?.color;
}

export const FONT_OPTIONS = [
  { value: 'geist', label: 'Geist' },
  { value: 'system', label: 'System' },
  { value: 'inter', label: 'Inter' },
] as const;

export function getFontLabel(value: string) {
  return FONT_OPTIONS.find((option) => option.value === value)?.label;
}

export const SEGMENTED_OPTIONS: Record<string, SegmentedOption[]> = {
  navbar: [
    { value: 'sticky', label: 'Sticky' },
    { value: 'scroll', label: 'Scroll' },
  ],
  sidebar: [
    { value: 'inset', label: 'Inset' },
    { value: 'sidebar', label: 'Sidebar' },
    { value: 'floating', label: 'Floating' },
  ],
  radius: [
    { value: '0', label: 'None', icon: CircleOff },
    { value: '0.375rem', label: 'SM' },
    { value: '0.625rem', label: 'MD' },
    { value: '1rem', label: 'LG' },
  ],
  scale: [
    { value: 'sm', label: 'SM' },
    { value: 'md', label: 'MD' },
    { value: 'lg', label: 'LG' },
  ],
};

export const SEGMENTED_FIELDS = [
  {
    key: 'navbarBehavior',
    label: 'Navbar Behavior',
    options: SEGMENTED_OPTIONS.navbar,
  },
  {
    key: 'sidebarStyle',
    label: 'Sidebar Style',
    options: SEGMENTED_OPTIONS.sidebar,
  },
  { key: 'scale', label: 'Scale', options: SEGMENTED_OPTIONS.scale },
] as const;
