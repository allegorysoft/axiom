import type {
  MenuNode,
  MenuGroup,
  MenuState,
  MenuPatch,
  MenuStore,
} from '../menu';

export interface Action<C = unknown> extends MenuNode<Action<C>> {
  action: () => void;
}

export type ActionGroup = MenuGroup<Action>;
export type ActionState = MenuState<Action>;
export type ActionPatch = MenuPatch<Action>;
export type ActionStore = MenuStore<ActionGroup, Action>;
