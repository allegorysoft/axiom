import { createStore } from '../store/axiom-store';
import { DEFAULT_MENU_GROUP } from '../menu/menu';
import { edit, find, findGroup } from '../menu/menu-utils';
import type { Tab, TabGroup, TabPatch, TabState, TabStore } from './tab';

const initialState: TabState = {
  groups: [{ title: DEFAULT_MENU_GROUP, isActive: false, children: [] }],
};

const baseStore = createStore<TabState>(initialState);

export const tabStore: TabStore = Object.assign({}, baseStore, {
  getGroup(title = DEFAULT_MENU_GROUP): TabGroup | undefined {
    return findGroup(baseStore.get().groups, title);
  },

  addGroup(title: string): TabGroup {
    const existing = findGroup(baseStore.get().groups, title);

    if (existing) {
      return existing;
    }

    const group: TabGroup = { title, isActive: false, children: [] };

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

  find(title: string, group = DEFAULT_MENU_GROUP): Tab | undefined {
    return find(getGroup(group)?.children ?? [], title);
  },

  add(item: Tab, group = DEFAULT_MENU_GROUP, parentTitle?: string): void {
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
          `tabStore.add: parent "${parentTitle}" not found in group "${group}"`,
        );
      }

      return updateGroup(prev, target, children);
    });
  },

  remove(title: string, group = DEFAULT_MENU_GROUP): void {
    updateNode(group, title, () => undefined);
  },

  update(title: string, patch: TabPatch, group = DEFAULT_MENU_GROUP): void {
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

function getGroup(title: string): TabGroup | undefined {
  return findGroup(baseStore.get().groups, title);
}

function updateGroup(
  state: TabState,
  group: TabGroup,
  children: Tab[],
): TabState {
  return {
    groups: state.groups.map((item) =>
      item === group ? { ...item, children } : item,
    ),
  };
}

function updateNode(
  group: string,
  title: string,
  update: (node: Tab) => Tab | undefined,
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
