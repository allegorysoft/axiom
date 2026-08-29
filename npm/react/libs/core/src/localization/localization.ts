import { AxiomStore } from '../models/common';

export type Translations = Record<string, Record<string, string>>;

export interface CultureInfo {
  /**
   * The culture code (e.g: `en-US`, `tr-TR`)
   */
  name: string;
  /**
   * The full culture name in your local language (e.g: `English (United States)`)
   */
  displayName: string;
}

export interface LocalizerState {
  translations: Translations;
  culture: CultureInfo;
  error: unknown;
}

export interface LocalizerStore extends AxiomStore<LocalizerState> {
  localize(
    key: string,
    args?: Record<string, unknown> | readonly unknown[],
  ): string;

  setCulture(culture: CultureInfo): void;
  setTranslations(incoming: Translations, overwrite?: boolean): void;

  reset(): void;
}
