import { AxiomStore } from '@axiomframework/react-core';

export type BaseSize = 'sm' | 'md' | 'lg';
export type RadiusSize = 'none' | BaseSize;

export type ThemePreset = {
  name: string;
  color: string;
};

export type Font = {
  value: string;
  title: string;
};

export type SidebarVariants = 'Inset' | 'Sidebar' | 'Floating';
export type NavbarBehavior = 'sticky' | 'scroll';

export interface Preferences {
  themePreset: ThemePreset;
  font: Font;
  navbarBehavior: NavbarBehavior;
  sidebarStyle: SidebarVariants;
  radius: {
    name: RadiusSize;
    value: string;
  };
  scale: BaseSize;
}

export interface PreferencesState {
  preferences: Preferences;
}

export interface PreferencesStore extends AxiomStore<PreferencesState> {
  setPreferences(preferences: Preferences): void;
  patchPreferences(preferences: Partial<Preferences>): void;
  resetPreferences(): void;
}
