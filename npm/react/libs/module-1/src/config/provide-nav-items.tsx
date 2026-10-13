import { HugeiconsIcon } from '@hugeicons/react';
import { UsersRoundIcon, UserSettings01Icon } from '@hugeicons/core-free-icons';

import { type NavGroup, navStore } from '@axiomframework/react-core';

import { MODULE_INFO } from './module';

export type NavItemsOptions = {
  skipLocalRegister: boolean;
};
export function provideNavItems(options?: NavItemsOptions) {
  navStore.addGroup(MODULE_INFO.name, { isLocal: !options?.skipLocalRegister } as NavGroup);
  navStore.add(
    {
      title: 'Entity-1',
      url: `/${MODULE_INFO.url}/entity-1`,
      icon: <HugeiconsIcon icon={UsersRoundIcon} strokeWidth={2} />,
    },
    MODULE_INFO.name,
  );
  navStore.add(
    {
      title: 'Entity-2',
      url: `/${MODULE_INFO.url}/entity-2`,
      icon: <HugeiconsIcon icon={UserSettings01Icon} strokeWidth={2} />,
    },
    MODULE_INFO.name,
  );
}
