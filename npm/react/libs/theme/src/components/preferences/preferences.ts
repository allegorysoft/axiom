import type { AxiomStore } from '@axiomframework/react-core';
import type {
  ColorThemeVariants,
  FontVariants,
  NavbarBehavior,
  SidebarVariants,
  BaseSize,
  RadiusValue,
} from './options';

export interface Preferences {
  colorTheme: ColorThemeVariants;
  font: FontVariants;
  navbarBehavior: NavbarBehavior;
  sidebarStyle: SidebarVariants;
  scale: BaseSize;
  radius: RadiusValue;
}

export interface PreferencesState {
  preferences: Preferences;
}

export interface PreferencesStore extends AxiomStore<PreferencesState> {
  setPreferences(preferences: Preferences): void;
  patchPreferences(preferences: Partial<Preferences>): void;
  resetPreferences(): void;
}
