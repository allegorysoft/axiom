import { createStoreHook } from '../../store/axiom-store';
import { DEFAULT_MENU_GROUP } from '../menu';
import type { Action, ActionGroup, ActionState, ActionStore } from './action';

const EMPTY_ITEMS: Action[] = [];

export function createActionHooks(store: ActionStore) {
  const useActionStore = createStoreHook<ActionState>(store);

  function useActionGroups(): ActionGroup[] {
    return useActionStore((state) => state.groups);
  }

  function useActionGroup(
    title: string = DEFAULT_MENU_GROUP,
  ): ActionGroup | undefined {
    return useActionStore((state) =>
      state.groups.find((group) => group.title === title),
    );
  }

  function useActionItems(title: string = DEFAULT_MENU_GROUP): Action[] {
    return useActionStore(
      (state) =>
        state.groups.find((group) => group.title === title)?.children ??
        EMPTY_ITEMS,
    );
  }

  return { useActionStore, useActionGroups, useActionGroup, useActionItems };
}
