import { HugeiconsIcon } from '@hugeicons/react';
import {
  UsersRoundIcon,
  UserSettings01Icon,
  Layers01Icon,
  File01Icon,
  HistoryIcon,
  Home03Icon as Home,
} from '@hugeicons/core-free-icons';
import { navStore } from '@axiomframework/react-core';

const PRODUCT = 'Identity Management';
const AUDIT = 'Audit';
const TENANT_MANAGEMENT = 'Tenant Management';

export function provideNavItems() {
  navStore.add({
    title: 'AxiomBase:Home',
    url: '/',
    icon: <HugeiconsIcon icon={Home} strokeWidth={2} />,
  });

  navStore.addGroup(PRODUCT, { isActive: true });
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

  navStore.addGroup(TENANT_MANAGEMENT, { isActive: true });
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
