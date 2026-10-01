import { createStore, createStoreHook } from '@axiomframework/react-core';
import { TENANTS } from './app-sidebar/data';

export type Tenant = {
  id: string;
  name: string;
  logo?: string;
  edition: string;
  expiration: string;
  active: string;
  shared: boolean;
  connection: string;
  current?: boolean;
};
export type Edition = { id: string; name: string };
const initialEditions = ['Standard', 'Professional', 'Enterprise'].map(
  (name) => ({ id: name, name }),
);
const names = [
  'Northstar Labs',
  'Atlas Commerce',
  'Lumen Works',
  'Harbor Systems',
  'Summit Health',
  'Cedar Finance',
  'Nova Retail',
  'Orchid Digital',
  'Pioneer Cloud',
  'Bluebird Media',
  'Willow Analytics',
  'Evergreen Studio',
  'Silverline Tech',
  'Maple Logistics',
  'Oceanic Ventures',
  'Vertex Solutions',
  'Aurora Design',
  'Meridian Group',
  'Horizon Energy',
  'Aspen Networks',
  'Beacon Software',
  'Cobalt Industries',
  'Delta Services',
  'Elm Research',
  'Falcon Systems',
];
const generatedTenants: Tenant[] = names.map((name, i) => ({
  id: String(i + 1),
  name,
  edition: initialEditions[i % 3].id,
  expiration:
    i % 4 === 0
      ? ''
      : `${2027 + (i % 3)}-${String((i % 12) + 1).padStart(2, '0')}-15`,
  active: i % 5 === 4 ? 'Passive' : 'Active',
  shared: true,
  connection: '',
  current: i === 0,
}));
const initialTenants: Tenant[] = [
  ...TENANTS.map((tenant, index) => ({
    ...tenant,
    expiration: '',
    active: 'Active',
    shared: true,
    connection: '',
    current: index === 0,
  })),
  ...generatedTenants
    .filter((tenant) => !TENANTS.some((item) => item.name === tenant.name))
    .map((tenant) => ({ ...tenant, current: false })),
];
// Preview data is shared between both pages, without storing credentials.
export const saasPreviewStore = createStore({
  tenants: initialTenants,
  editions: initialEditions,
});
export const useSaasPreview = createStoreHook(saasPreviewStore);
export function newTenant(): Tenant {
  return {
    id: '',
    name: '',
    edition: saasPreviewStore.get().editions[0]?.id ?? '',
    expiration: '',
    active: 'Active',
    shared: true,
    connection: '',
  };
}
export function saveTenant(item: Tenant): string | undefined {
  const data = saasPreviewStore.get();
  if (
    data.tenants.some(
      (t) =>
        t.id !== item.id && t.name.toLowerCase() === item.name.toLowerCase(),
    )
  )
    return 'This tenant name is already in use.';
  saasPreviewStore.set(() => ({
    tenants: item.id
      ? data.tenants.map((t) => (t.id === item.id ? item : t))
      : [{ ...item, id: crypto.randomUUID() }, ...data.tenants],
  }));
  return undefined;
}
