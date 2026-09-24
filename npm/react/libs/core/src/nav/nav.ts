import type {
  MenuNode,
  MenuGroup,
  MenuState,
  MenuPatch,
  MenuStore,
} from '../menu/menu';

export interface Nav extends MenuNode<Nav> {
  url?: string;
}

export type NavGroup = MenuGroup<Nav>;

export type NavState = MenuState<Nav>;
export type NavPatch = MenuPatch<Nav>;
export type NavStore = MenuStore<Nav>;
