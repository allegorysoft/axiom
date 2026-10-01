import type { Nav } from './nav';

export type NavPanelState = {
  readonly panels: readonly Nav[];
  readonly depth: number;
};

export type NavPanelAction =
  { type: 'enter'; item: Nav } | { type: 'back' } | { type: 'reset' };

export const initialNavPanelState: NavPanelState = { panels: [], depth: 0 };

export function navPanelReducer(
  state: NavPanelState,
  action: NavPanelAction,
): NavPanelState {
  switch (action.type) {
    case 'enter': {
      const item = action.item;
      if (item.childrenDisplay !== 'panel' || !item.children?.length) {
        return state;
      }

      const parent = state.panels[state.depth - 1];
      if (parent && !parent.children?.includes(item)) {
        return state;
      }

      return {
        panels: [...state.panels.slice(0, state.depth), item],
        depth: state.depth + 1,
      };
    }
    case 'back':
      return state.depth ? { ...state, depth: state.depth - 1 } : state;
    case 'reset':
      return initialNavPanelState;
  }
}
