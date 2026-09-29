import { ACCOUNT_DATA } from './account';
import { APPEARANCE_DATA } from './apperance';
import { LANGUAGE_REGION_SECTIONS } from './language-and-region';

export const USER_SETTINGS_SECTIONS = [
  ACCOUNT_DATA.profile,
  ACCOUNT_DATA.accountInfo,
  ACCOUNT_DATA.security,
  APPEARANCE_DATA.layout,
  APPEARANCE_DATA.theme,
  APPEARANCE_DATA.typography,
  LANGUAGE_REGION_SECTIONS.language,
  LANGUAGE_REGION_SECTIONS.timeZone,
] as const;
