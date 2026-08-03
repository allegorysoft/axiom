import { AxiomStore } from './common';

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

export type LocalizerStatus = 'idle' | 'loading' | 'ready' | 'error';

export interface LocalizerState {
  translations: Translations;
  culture: CultureInfo;
  status: LocalizerStatus;
  error: unknown;
}

export interface LocalizerStore extends AxiomStore<LocalizerState> {
  localize(
    key: string,
    moduleName?: string,
    args?: Record<string, unknown> | readonly unknown[],
  ): string;

  setTranslations(incoming: Translations, overwrite?: boolean): void;
  setCulture(culture: CultureInfo): void;
  setStatus(status: LocalizerStatus, error?: unknown): void;

  reset(): void;
}
