import type { ReactNode } from 'react';
import type { AxiomStore } from '../models/common';

export const DEFAULT_MENU_GROUP = 'Default';

export const enum MenuPriority {
  High = 1,
  Medium = 0,
  Low = -1,
}

export interface MenuGroup<C = MenuNode<unknown>> {
  title: string;
  active?: boolean;
  mode?: 'default' | 'collapsible';
  children: C[];
  priority?: MenuPriority | number;
}

export interface MenuNode<C> {
  icon?: ReactNode;
  title: string;
  badge?: string;
  permission?: string;
  active?: boolean;
  children?: C[];
  priority?: MenuPriority | number;
}

export type MenuState<TNode> = { groups: MenuGroup<TNode>[] };

export type MenuPatch<TNode> =
  Partial<TNode> | ((node: TNode) => Partial<TNode>);

export type MenuStore<TGroup extends MenuGroup<TNode>, TNode> = AxiomStore<MenuState<TNode>> & {
  getGroup(title?: string): TGroup | undefined;
  addGroup(title: string, options?: Partial<TGroup>): TGroup;
  patchGroup(title: string, patched: Partial<TGroup>): TGroup;
  removeGroup(title: string): void;
  find(title: string, group?: string): TNode | undefined;
  add(item: TNode, group?: string, parentTitle?: string): void;
  remove(title: string, group?: string): void;
  update(title: string, patch: MenuPatch<TNode>, group?: string): void;
  toggle(title: string, group?: string): void;
};
