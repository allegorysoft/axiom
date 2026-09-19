import { type IconSvgElement } from '@hugeicons/react';
import { CircleOffIcon as CircleOff } from '@hugeicons/core-free-icons';

export type Option<T = string> = {
  readonly value: T;
  readonly label: string;
  readonly icon?: IconSvgElement;
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

export const COLOR_THEME_OPTIONS = [
  { value: 'default', label: 'Default', color: '#000000' },
  { value: 'axiom', label: 'Axiom', color: '#0096FC' },
  { value: 'amber', label: 'Amber', color: '#f59e0b' },
  { value: 'amethyst', label: 'Amethyst', color: '#8c5cff' },
  { value: 'bubblegum', label: 'Bubblegum', color: '#c67b96' },
  { value: 'caffeine', label: 'Caffeine', color: '#FCDFC2' },
  { value: 'claude', label: 'Claude', color: '#D87657' },
  { value: 'crimson', label: 'Crimson', color: '#D40C1A' },
  { value: 'cyberpunk', label: 'Cyberpunk', color: '#ff00c8' },
  { value: 'ghibli-studio', label: 'Ghibli Studio', color: '#8A906E' },
  { value: 'nature', label: 'Nature', color: '#4dae50' },
  { value: 'rose', label: 'Rose', color: '#d87bac' },
  { value: 'soft-pop', label: 'Soft Pop', color: '#2dd4bf' },
  { value: 'tangerine', label: 'Tangerine', color: '#e05d38' },
  { value: 'wintry', label: 'Wintry', color: '#0265FD' },
] as const;

export const COLOR_THEME_CLASSES: readonly ColorThemeName[] = COLOR_THEME_OPTIONS.map(
  (colorTheme) => colorTheme.value,
);

export const FONT_OPTIONS = [
  { value: 'system', label: 'System' },
  { value: 'geist', label: 'Geist' },
  { value: 'manrope', label: 'Manrope' },
  { value: 'noto-serif', label: 'Noto Serif' },
  { value: 'jetbrains-mono', label: 'JetBrains Mono' },
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

export type ColorThemeName = (typeof COLOR_THEME_OPTIONS)[number]['value'];
export type FontName = (typeof FONT_OPTIONS)[number]['value'];

type SegmentedOptions = typeof SEGMENTED_OPTIONS;

export type NavbarBehavior = SegmentedOptions['navbar'][number]['value'];
export type SidebarVariants = SegmentedOptions['sidebar'][number]['value'];
export type RadiusValue = SegmentedOptions['radius'][number]['value'];
export type BaseSize = SegmentedOptions['scale'][number]['value'];
