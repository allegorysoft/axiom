import type { ComponentType, LazyExoticComponent } from 'react';
import type {
  MenuNode,
  MenuGroup,
  MenuState,
  MenuPatch,
  MenuStore,
} from '../menu/menu';

export interface Tab<C = unknown> extends MenuNode<Tab<C>> {
  component: LazyExoticComponent<ComponentType<C>> | ComponentType<C>;
}

export type TabGroup = MenuGroup<Tab>;
export type TabState = MenuState<Tab>;
export type TabPatch = MenuPatch<Tab>;
export type TabStore = MenuStore<Tab>;
