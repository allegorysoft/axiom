import { HugeiconsIcon } from '@hugeicons/react';
import {
  UsersRoundIcon,
  UserSettings01Icon,
  Layers01Icon,
  File01Icon,
  HistoryIcon,
  Home03Icon as Home,
} from '@hugeicons/core-free-icons';
import { DEFAULT_MENU_GROUP, navStore } from '@axiomframework/react-core';

const IDENTITY_MANAGEMENT = 'Identity Management';
const AUDIT = 'Audit';
const TENANT_MANAGEMENT = 'Tenant Management';

const TEST = 'Test';

export function provideNavItems() {
  navStore.add({
    title: 'AxiomBase:Home',
    url: '/',
    icon: <HugeiconsIcon icon={Home} strokeWidth={2} />,
  });

  navStore.addGroup(IDENTITY_MANAGEMENT, { isActive: true });
  navStore.add(
    {
      title: 'Users',
      url: '/identity-management/users',
      icon: <HugeiconsIcon icon={UsersRoundIcon} strokeWidth={2} />,
    },
    IDENTITY_MANAGEMENT,
  );
  navStore.add(
    {
      title: 'Roles',
      url: '/identity-management/roles',
      icon: <HugeiconsIcon icon={UserSettings01Icon} strokeWidth={2} />,
    },
    IDENTITY_MANAGEMENT,
  );

  navStore.addGroup(TENANT_MANAGEMENT);
  navStore.add(
    {
      title: 'Tenants',
      url: '/tenant-management/tenants',
      icon: <HugeiconsIcon icon={UsersRoundIcon} strokeWidth={2} />,
    },
    TENANT_MANAGEMENT,
  );
  navStore.add(
    {
      title: 'Editions',
      url: '/tenant-management/editions',
      icon: <HugeiconsIcon icon={Layers01Icon} strokeWidth={2} />,
    },
    TENANT_MANAGEMENT,
  );

  navStore.addGroup(TEST, { isActive: true });
  navStore.add(
    {
      title: 'Test Management',
      mode: 'switch-panel',
      icon: <HugeiconsIcon icon={File01Icon} strokeWidth={2} />,
      children: [
        {
          title: DEFAULT_MENU_GROUP,
          children: [{ title: 'Overview' }],
        },
        {
          title: 'Users',
          children: [{ title: 'List' }, { title: 'Detail' }],
        },
        {
          title: 'Billing',
          mode: 'switch-panel', // nested panels work — each has its own rootTitle
          children: [
            { title: 'Invoices' },
            { title: 'Payments', children: [{ title: 'Overview' }] },
            {
              title: 'Billing-Child',
              mode: 'switch-panel', // nested panels work — each has its own rootTitle
              children: [{ title: 'Payments' }],
            },
          ],
        },
      ],
    },
    TEST,
  );

  navStore.addGroup(AUDIT, { isActive: true });
  navStore.add(
    {
      title: 'Audit Logs',
      url: '/audit/logs',
      icon: <HugeiconsIcon icon={File01Icon} strokeWidth={2} />,
    },
    AUDIT,
  );
  navStore.add(
    {
      title: 'Entity Changes',
      url: '/audit/entity-changes',
      icon: <HugeiconsIcon icon={HistoryIcon} strokeWidth={2} />,
    },
    AUDIT,
  );
}
