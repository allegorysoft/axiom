import { ACCOUNT_DATA } from '../profile/account';
import { APPEARANCE_DATA } from '../profile/apperance';
import { LANGUAGE_REGION_SECTIONS } from '../profile/language-and-region';

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
