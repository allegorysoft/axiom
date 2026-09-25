import { type IconSvgElement } from '@hugeicons/react';
import { CircleOffIcon as CircleOff } from '@hugeicons/core-free-icons';

export type Option<T = string> = {
  readonly value: T;
  readonly label: string;
  readonly icon?: IconSvgElement;
};

export function getOption<T extends Option>(
  options: readonly T[],
  value: T['value'],
): T | undefined {
  return options.find((option) => option.value === value);
}

export const COLOR_THEME_OPTIONS = [
  { value: 'default',         label: 'Default',      color: '#000000', url: '' },
  { value: 'axiom',           label: 'Axiom',        color: '#0096FC', url: new URL('../../styles/axiom.css?no-inline',         import.meta.url).href },
  { value: 'amber',           label: 'Amber',        color: '#f59e0b', url: new URL('../../styles/amber.css?no-inline',         import.meta.url).href },
  { value: 'amethyst',        label: 'Amethyst',     color: '#8c5cff', url: new URL('../../styles/amethyst.css?no-inline',      import.meta.url).href },
  { value: 'bubblegum',       label: 'Bubblegum',    color: '#c67b96', url: new URL('../../styles/bubblegum.css?no-inline',     import.meta.url).href },
  { value: 'caffeine',        label: 'Caffeine',     color: '#FCDFC2', url: new URL('../../styles/caffeine.css?no-inline',      import.meta.url).href },
  { value: 'claude',          label: 'Claude',       color: '#D87657', url: new URL('../../styles/claude.css?no-inline',        import.meta.url).href },
  { value: 'crimson',         label: 'Crimson',      color: '#D40C1A', url: new URL('../../styles/crimson.css?no-inline',       import.meta.url).href },
  { value: 'cyberpunk',       label: 'Cyberpunk',    color: '#ff00c8', url: new URL('../../styles/cyberpunk.css?no-inline',     import.meta.url).href },
  { value: 'ghibli-studio',   label: 'Ghibli Studio',color: '#8A906E', url: new URL('../../styles/ghibli-studio.css?no-inline', import.meta.url).href },
  { value: 'nature',          label: 'Nature',       color: '#4dae50', url: new URL('../../styles/nature.css?no-inline',        import.meta.url).href },
  { value: 'rose',            label: 'Rose',         color: '#d87bac', url: new URL('../../styles/rose.css?no-inline',          import.meta.url).href },
  { value: 'seafoam',         label: 'Seafoam',      color: '#0D8989', url: new URL('../../styles/seafoam.css?no-inline',       import.meta.url).href },
  { value: 'soft-pop',        label: 'Soft Pop',     color: '#FFC716', url: new URL('../../styles/soft-pop.css?no-inline',      import.meta.url).href },
  { value: 'tangerine',       label: 'Tangerine',    color: '#e05d38', url: new URL('../../styles/tangerine.css?no-inline',     import.meta.url).href },
  { value: 'wintry',          label: 'Wintry',       color: '#0265FD', url: new URL('../../styles/wintry.css?no-inline',        import.meta.url).href },
] as const;


export const FONT_OPTIONS = [
  // Sans Serif
  {
    value: 'system',
    label: 'System',
    group: 'Sans Serif',
    url: undefined,
  },
  {
    value: 'figtree',
    label: 'Figtree',
    group: 'Sans Serif',
    url: 'https://fonts.googleapis.com/css2?family=Figtree:wght@400;500;600;700;800&display=swap',
  },
  {
    value: 'geist',
    label: 'Geist',
    group: 'Sans Serif',
    url: 'https://fonts.googleapis.com/css2?family=Geist:wght@400;500;600;700;800&display=swap',
  },
  {
    value: 'inter',
    label: 'Inter',
    group: 'Sans Serif',
    url: 'https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap',
  },
  {
    value: 'manrope',
    label: 'Manrope',
    group: 'Sans Serif',
    url: 'https://fonts.googleapis.com/css2?family=Manrope:wght@400;500;600;700;800&display=swap',
  },
  {
    value: 'montserrat',
    label: 'Montserrat',
    group: 'Sans Serif',
    url: 'https://fonts.googleapis.com/css2?family=Montserrat:wght@400;500;600;700;800&display=swap',
  },
  {
    value: 'plus-jakarta-sans',
    label: 'Plus Jakarta Sans',
    group: 'Sans Serif',
    url: 'https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;500;600;700;800&display=swap',
  },
  {
    value: 'poppins',
    label: 'Poppins',
    group: 'Sans Serif',
    url: 'https://fonts.googleapis.com/css2?family=Poppins:wght@400;500;600;700;800&display=swap',
  },

  // Serif
  {
    value: 'aleo',
    label: 'Aleo',
    group: 'Serif',
    url: 'https://fonts.googleapis.com/css2?family=Aleo:wght@400;500;600;700;800&display=swap',
  },
  {
    value: 'noto-serif',
    label: 'Noto Serif',
    group: 'Serif',
    url: 'https://fonts.googleapis.com/css2?family=Noto+Serif:wght@400;500;600;700;800&display=swap',
  },
  {
    value: 'playfair',
    label: 'Playfair',
    group: 'Serif',
    url: 'https://fonts.googleapis.com/css2?family=Playfair+Display:wght@400;500;600;700;800&display=swap',
  },

  // Monospace
  {
    value: 'ibm-plex-mono',
    label: 'IBM Plex Mono',
    group: 'Monospace',
    url: 'https://fonts.googleapis.com/css2?family=IBM+Plex+Mono:wght@400;500;600;700&display=swap',
  },
  {
    value: 'jetbrains-mono',
    label: 'JetBrains Mono',
    group: 'Monospace',
    url: 'https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@400;500;600;700;800&display=swap',
  },
  {
    value: 'source-code-pro',
    label: 'Source Code Pro',
    group: 'Monospace',
    url: 'https://fonts.googleapis.com/css2?family=Source+Code+Pro:wght@400;500;600;700;800&display=swap',
  },
  {
    value: 'space-mono',
    label: 'Space Mono',
    group: 'Monospace',
    url: 'https://fonts.googleapis.com/css2?family=Space+Mono:wght@400;700&display=swap',
  },
];

export const SEGMENTED_OPTIONS = {
  theme: [
    { value: 'system', label: 'AxiomTheme:System' },
    { value: 'light', label: 'AxiomTheme:Light' },
    { value: 'dark', label: 'AxiomTheme:Dark' },
  ],
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
} as const satisfies Record<string, readonly Option[]>;

export type ColorThemeVariants = (typeof COLOR_THEME_OPTIONS)[number]['value'];
export type FontVariants = (typeof FONT_OPTIONS)[number]['value'];

type SegmentedOptions = typeof SEGMENTED_OPTIONS;
export type NavbarBehavior = SegmentedOptions['navbar'][number]['value'];
export type SidebarVariants = SegmentedOptions['sidebar'][number]['value'];
export type RadiusValue = SegmentedOptions['radius'][number]['value'];
export type BaseSize = SegmentedOptions['scale'][number]['value'];
