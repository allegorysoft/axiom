import { lazy } from 'react';
import { Tab, tabStore } from '@axiomframework/react-core';

export function configureTheme() {
  provideProfileTabs();
}

function provideProfileTabs() {
  const Account = 'Account';
  tabStore.addGroup(Account);

  const tab: Tab = {
    //icon: SquareUserRoundIcon,
    title: 'Profile',
    component: lazy(() => new Promise(MyComponent)),
  };

  tabStore.add(tab, Account);
}

function MyComponent() {
  return 'Profile component';
}
