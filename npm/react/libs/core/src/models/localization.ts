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
