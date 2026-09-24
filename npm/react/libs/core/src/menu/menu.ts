import type { ReactNode } from 'react';
import type { AxiomStore } from '../models/common';

export const DEFAULT_MENU_GROUP = 'Default';

export interface MenuGroup<C = MenuNode<unknown>> {
  title: string;
  isActive: boolean;
  children: C[];
}

export interface MenuNode<C> {
  icon?: ReactNode;
  title: string;
  badge?: string;
  permission?: string;
  isActive?: boolean;
  children?: C[];
}

export type MenuState<TNode> = { groups: MenuGroup<TNode>[] };

export type MenuPatch<TNode> =
  Partial<TNode> | ((node: TNode) => Partial<TNode>);

export type MenuStore<TNode> = AxiomStore<MenuState<TNode>> & {
  getGroup(title?: string): MenuGroup<TNode> | undefined;
  addGroup(title: string): MenuGroup<TNode>;
  removeGroup(title: string): void;
  find(title: string, group?: string): TNode | undefined;
  add(item: TNode, group?: string, parentTitle?: string): void;
  remove(title: string, group?: string): void;
  update(title: string, patch: MenuPatch<TNode>, group?: string): void;
  toggle(title: string, group?: string): void;
};
