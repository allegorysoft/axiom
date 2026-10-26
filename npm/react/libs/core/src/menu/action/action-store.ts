import { createStore } from '../../store/axiom-store';
import { DEFAULT_MENU_GROUP } from '../menu';
import { edit, find, findGroup } from '../menu-utils';
import type {
  Action,
  ActionGroup,
  ActionPatch,
  ActionState,
  ActionStore,
} from './action';

function createInitialState(): ActionState {
  return {
    groups: [{ title: DEFAULT_MENU_GROUP, isActive: false, children: [] }],
  };
}

export function createActionStore(): ActionStore {
  const baseStore = createStore<ActionState>(createInitialState());

  function getGroup(title: string): ActionGroup | undefined {
    return findGroup(baseStore.get().groups, title);
  }

  function updateGroup(
    state: ActionState,
    group: ActionGroup,
    children: Action[],
  ): ActionState {
    return {
      groups: state.groups.map((item) =>
        item === group ? { ...item, children } : item,
      ),
    };
  }

  function updateNode(
    group: string,
    title: string,
    update: (node: Action) => Action | undefined,
  ): void {
    baseStore.set((prev) => {
      const target = findGroup(prev.groups, group);

      if (!target) {
        return prev;
      }

      const children = edit(target.children, title, update);

      return children === target.children
        ? prev
        : updateGroup(prev, target, children);
    });
  }

  return Object.assign({}, baseStore, {
    getGroup(title = DEFAULT_MENU_GROUP): ActionGroup | undefined {
      return findGroup(baseStore.get().groups, title);
    },
    addGroup(title: string, options?: Partial<ActionGroup>): ActionGroup {
      const existing = findGroup(baseStore.get().groups, title);

      if (existing) {
        return existing;
      }

      const group: ActionGroup = {
        title,
        isActive: false,
        children: [],
        ...options,
      };

      baseStore.set((prev) => ({
        groups: [...prev.groups, group],
      }));

      return group;
    },
    removeGroup(title: string): void {
      baseStore.set((prev) => {
        const groups = prev.groups.filter((group) => group.title !== title);

        return groups.length === prev.groups.length ? prev : { groups };
      });
    },
    patchGroup(title: string, patched: Partial<ActionGroup>): ActionGroup {
      const group = findGroup(baseStore.get().groups, title);

      if (!group) {
        throw new Error(`navStore.patchGroup: group "${title}" not found`);
      }

      const patchedGroup: ActionGroup = { ...group, ...patched };

      baseStore.set((prev) => ({
        groups: prev.groups.map((item) =>
          item === group ? patchedGroup : item,
        ),
      }));

      return patchedGroup;
    },

    find(title: string, group = DEFAULT_MENU_GROUP): Action | undefined {
      return find(getGroup(group)?.children ?? [], title);
    },
    add(item: Action, group = DEFAULT_MENU_GROUP, parentTitle?: string): void {
      baseStore.set((prev) => {
        const target = findGroup(prev.groups, group);

        if (!target) {
          return {
            groups: [
              ...prev.groups,
              {
                title: group,
                isActive: false,
                children: [item],
              },
            ],
          };
        }

        if (!parentTitle) {
          return updateGroup(prev, target, [...target.children, item]);
        }

        const children = edit(target.children, parentTitle, (parent) => ({
          ...parent,
          children: [...(parent.children ?? []), item],
        }));

        if (children === target.children) {
          throw new Error(
            `actionStore.add: parent "${parentTitle}" not found in group "${group}"`,
          );
        }

        return updateGroup(prev, target, children);
      });
    },
    remove(title: string, group = DEFAULT_MENU_GROUP): void {
      updateNode(group, title, () => undefined);
    },
    update(title: string, patch: ActionPatch, group = DEFAULT_MENU_GROUP): void {
      updateNode(group, title, (node) => ({
        ...node,
        ...(typeof patch === 'function' ? patch(node) : patch),
      }));
    },
    toggle(title: string, group = DEFAULT_MENU_GROUP): void {
      updateNode(group, title, (node) => ({
        ...node,
        isActive: !node.isActive,
      }));
    },
  });
}
