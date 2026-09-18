import { CircleOff, type LucideIcon } from 'lucide-react';

export type Option<T extends string = string> = {
  readonly value: T;
  readonly label: string;
  readonly icon?: LucideIcon;
};

export function getOption<T extends Option>(
  options: readonly T[],
  value: T['value'],
): T | undefined {
  return options.find((option) => option.value === value);
}

export const PRESET_OPTIONS = [
  { value: 'default', label: 'Default', color: '#000000' },
  { value: 'neutral', label: 'Neutral', color: '#808080' },
  { value: 'vibrant', label: 'Vibrant', color: '#FF0000' },
] as const;

export type PresetOption = (typeof PRESET_OPTIONS)[number];
export type PresetName = PresetOption['value'];

export const PRESET_CLASSES: readonly PresetName[] = PRESET_OPTIONS.map(
  (preset) => preset.value,
);

export const FONT_OPTIONS = [
  { value: 'geist', label: 'Geist' },
  { value: 'system', label: 'System' },
  { value: 'inter', label: 'Inter' },
] as const;

export type FontOption = (typeof FONT_OPTIONS)[number];
export type FontName = FontOption['value'];

export const SEGMENTED_OPTIONS = {
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
} as const satisfies Record<string, readonly Option[]>;

export type SegmentedOptions = typeof SEGMENTED_OPTIONS;

export type NavbarBehavior = SegmentedOptions['navbar'][number]['value'];
export type SidebarVariants = SegmentedOptions['sidebar'][number]['value'];
export type RadiusValue = SegmentedOptions['radius'][number]['value'];
export type BaseSize = SegmentedOptions['scale'][number]['value'];

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
