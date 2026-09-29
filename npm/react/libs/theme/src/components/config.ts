import { Tab, tabStore } from '@axiomframework/react-core';
import {
  LanguagesIcon,
  Layout01Icon,
  PaletteIcon,
  SquareUserRoundIcon,
  TimeZoneIcon,
  UserAccountIcon,
} from '@hugeicons/core-free-icons';

export function configureTheme() {
  provideProfileTabs();
}

function provideProfileTabs() {
  const Account = 'Account';
  tabStore.addGroup(Account);

  const profileTab: Tab = {
    icon: SquareUserRoundIcon as any,
    title: 'Profile',
    component: MyComponent,
  };

  tabStore.add(profileTab, Account);

  const accountDetailsTab: Tab = {
    icon: UserAccountIcon as any,
    title: 'Account Details',
    component: MyComponent,
  };

  tabStore.add(accountDetailsTab, Account);

  const Appearance = 'Appearance';
  tabStore.addGroup(Appearance);

  const appearanceLayoutTab: Tab = {
    icon: Layout01Icon as any,
    title: 'Layout',
    component: MyComponent,
  };

  tabStore.add(appearanceLayoutTab, Appearance);

  const appearanceThemeTab: Tab = {
    icon: PaletteIcon as any,
    title: 'Theme',
    component: MyComponent,
  };

  tabStore.add(appearanceThemeTab, Appearance);

  const LanguageRegion = 'Language & Region';
  tabStore.addGroup(LanguageRegion);

  const languageRegionLanguageTab: Tab = {
    icon: LanguagesIcon as any,
    title: 'Language',
    component: MyComponent,
  };

  tabStore.add(languageRegionLanguageTab, LanguageRegion);

  const languageRegionTimeZoneTab: Tab = {
    icon: TimeZoneIcon as any,
    title: 'Time Zone',
    component: MyComponent,
  };

  tabStore.add(languageRegionTimeZoneTab, LanguageRegion);
}

function MyComponent() {
  return 'Component';
}
