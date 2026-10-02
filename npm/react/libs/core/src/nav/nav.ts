import type {
  MenuNode,
  MenuGroup,
  MenuState,
  MenuPatch,
  MenuStore,
} from '../menu/menu';

export interface Nav extends MenuNode<Nav> {
  url?: string;
  mode?: 'default' | 'switch-panel';
  /** Groups to render when this node is opened in a panel. Only meaningful together with `mode: switch-panel`. */
  groups?: NavGroup[];
}

export type NavGroup = MenuGroup<Nav>;
export type NavState = MenuState<Nav>;
export type NavPatch = MenuPatch<Nav>;
export type NavStore = MenuStore<Nav>;
