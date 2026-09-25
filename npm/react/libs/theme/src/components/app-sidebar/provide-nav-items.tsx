import { HugeiconsIcon } from '@hugeicons/react';
import {
  UsersRoundIcon,
  UserSettings01Icon,
  Layers01Icon,
  File01Icon,
  HistoryIcon,
  Home03Icon as Home,
  DashboardBrowsingIcon,
} from '@hugeicons/core-free-icons';
import { navStore } from '@axiomframework/react-core';

const PRODUCT = 'Identity Management';
const AUDIT = 'Audit';
const SAAS = 'Tenant Management';

export function provideNavItems() {
  navStore.add({
    title: 'AxiomBase:Dashboard',
    url: '/',
    icon: <HugeiconsIcon icon={DashboardBrowsingIcon} strokeWidth={2} />,
  });
  navStore.add({
    title: 'AxiomBase:Home',
    url: '/home',
    icon: <HugeiconsIcon icon={Home} strokeWidth={2} />,
  });

  navStore.addGroup(PRODUCT);
  navStore.add(
    {
      title: 'Users',
      url: '/identity-management/users',
      icon: <HugeiconsIcon icon={UsersRoundIcon} strokeWidth={2} />,
    },
    PRODUCT,
  );
  navStore.add(
    {
      title: 'Roles',
      url: '/identity-management/roles',
      icon: <HugeiconsIcon icon={UserSettings01Icon} strokeWidth={2} />,
    },
    PRODUCT,
  );

  navStore.addGroup(SAAS);
  navStore.add(
    {
      title: 'Tenants',
      url: '/saas-management/tenants',
      icon: <HugeiconsIcon icon={UsersRoundIcon} strokeWidth={2} />,
    },
    SAAS,
  );
  navStore.add(
    {
      title: 'Editions',
      url: '/saas-management/editions',
      icon: <HugeiconsIcon icon={Layers01Icon} strokeWidth={2} />,
    },
    SAAS,
  );

  navStore.addGroup(AUDIT);
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
