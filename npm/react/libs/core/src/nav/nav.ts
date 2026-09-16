import type { JSX } from 'react';
import type { AxiomStore } from '../models/common';

export const DEFAULT_NAV_GROUP = 'Default';

export type Nav = {
  title: string;
  url?: string;
  icon?: JSX.Element;
  badge?: string;
  permission?: string | null;
  isActive?: boolean;
  children?: Nav[];
};

export type NavGroup = {
  title: string;
  isActive: boolean;
  items: Nav[];
};

export type NavState = {
  groups: NavGroup[];
};

export type NavPatch = Partial<Nav> | ((node: Nav) => Partial<Nav>);

export type NavStore = AxiomStore<NavState> & {
  getGroup(title?: string): NavGroup | undefined;
  addGroup(title: string): NavGroup;
  removeGroup(title: string): void;
  find(title: string, group?: string): Nav | undefined;
  add(item: Nav, group?: string, parentTitle?: string): void;
  remove(title: string, group?: string): void;
  update(title: string, patch: NavPatch, group?: string): void;
  toggle(title: string, group?: string): void;
};
