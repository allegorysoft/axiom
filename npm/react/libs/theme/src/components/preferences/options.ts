import { CircleOff, type LucideIcon } from 'lucide-react';

export type Option<T = string> = {
  readonly value: T;
  readonly label: string;
  readonly icon?: LucideIcon;
};

type ScaleOption<T = string> = Option<T> & {
  readonly scale: number;
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

export const PRESET_CLASSES: readonly PresetName[] = PRESET_OPTIONS.map(
  (preset) => preset.value,
);

export const FONT_OPTIONS = [
  { value: 'system', label: 'System' },
  { value: 'geist', label: 'Geist' },
  { value: 'manrope', label: 'Manrope' },
];

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
    { value: 'none', label: 'None', icon: CircleOff },
    { value: 'sm', label: 'SM' },
    { value: 'md', label: 'MD' },
    { value: 'lg', label: 'LG' },
  ],
  scale: [
    { value: 'sm', label: 'SM' },
    { value: 'md', label: 'MD' },
    { value: 'lg', label: 'LG' },
  ],
} satisfies Record<string, readonly Option[] | readonly ScaleOption[]>;

export type PresetName = (typeof PRESET_OPTIONS)[number]['value'];
export type FontName = (typeof FONT_OPTIONS)[number]['value'];

type SegmentedOptions = typeof SEGMENTED_OPTIONS;

export type NavbarBehavior = SegmentedOptions['navbar'][number]['value'];
export type SidebarVariants = SegmentedOptions['sidebar'][number]['value'];
export type RadiusValue = SegmentedOptions['radius'][number]['value'];
export type BaseSize = SegmentedOptions['scale'][number]['value'];
