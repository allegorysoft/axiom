import { AxiomStore } from '@axiomframework/react-core';
import type {
  BaseSize,
  FontName,
  NavbarBehavior,
  ColorThemeName,
  RadiusValue,
  SidebarVariants,
} from './options';

export interface Preferences {
  colorTheme: ColorThemeName;
  font: FontName;
  navbarBehavior: NavbarBehavior;
  sidebarStyle: SidebarVariants;
  radius: RadiusValue;
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
