import { createStore } from '../store/axiom-store';
import type { Nav, NavGroup, NavPatch, NavState, NavStore } from './nav';
import { edit, find, selectGroup } from './nav-utils';

export const DEFAULT_NAV_GROUP = 'Default';

const initialState: NavState = {
  groups: [{ title: DEFAULT_NAV_GROUP, isActive: false, items: [] }],
};

const baseStore = createStore<NavState>(initialState);

export const navStore: NavStore = Object.assign({}, baseStore, {
  getGroup(title: string = DEFAULT_NAV_GROUP): NavGroup | undefined {
    return selectGroup(baseStore.get().groups, title);
  },

  addGroup(title: string): NavGroup {
    const existing = selectGroup(baseStore.get().groups, title);

    if (existing) {
      return existing;
    }

    const group: NavGroup = { title, isActive: false, items: [] };

    baseStore.set((prev) => ({ groups: [...prev.groups, group] }));

    return group;
  },

  removeGroup(title: string): void {
    baseStore.set((prev) => {
      if (!prev.groups.some((group) => group.title === title)) {
        return prev;
      }
      return { groups: prev.groups.filter((group) => group.title !== title) };
    });
  },

  find(title: string, group: string = DEFAULT_NAV_GROUP): Nav | undefined {
    return find(selectGroup(baseStore.get().groups, group)?.items ?? [], title);
  },

  add(
    item: Nav,
    group: string = DEFAULT_NAV_GROUP,
    parentTitle?: string,
  ): void {
    baseStore.set((prev) => {
      const target = selectGroup(prev.groups, group);

      if (!target) {
        return {
          groups: [
            ...prev.groups,
            {
              title: group,
              isActive: false,
              items: [item],
            },
          ],
        };
      }

      if (parentTitle) {
        const items = edit(target.items, parentTitle, (parent) => ({
          ...parent,
          children: [...(parent.children ?? []), item],
        }));

        if (items === target.items) {
          throw new Error(
            `navStore.add: parent "${parentTitle}" not found in group "${group}"`,
          );
        }

        return {
          groups: prev.groups.map((g) =>
            g.title === group ? { ...g, items } : g,
          ),
        };
      }

      return {
        groups: prev.groups.map((g) =>
          g.title === group ? { ...g, items: [...g.items, item] } : g,
        ),
      };
    });
  },

  remove(title: string, group: string = DEFAULT_NAV_GROUP): void {
    baseStore.set((prev) => {
      const target = selectGroup(prev.groups, group);

      if (!target) {
        return prev;
      }

      const items = edit(target.items, title, () => undefined);

      if (items === target.items) {
        return prev;
      }

      return {
        groups: prev.groups.map((g) =>
          g.title === group ? { ...g, items } : g,
        ),
      };
    });
  },

  update(
    title: string,
    patch: NavPatch,
    group: string = DEFAULT_NAV_GROUP,
  ): void {
    baseStore.set((prev) => {
      const target = selectGroup(prev.groups, group);

      if (!target) {
        return prev;
      }

      const items = edit(target.items, title, (node) => ({
        ...node,
        ...(typeof patch === 'function' ? patch(node) : patch),
      }));

      if (items === target.items) {
        return prev;
      }

      return {
        groups: prev.groups.map((g) =>
          g.title === group ? { ...g, items } : g,
        ),
      };
    });
  },

  toggle(title: string, group: string = DEFAULT_NAV_GROUP): void {
    baseStore.set((prev) => {
      const target = selectGroup(prev.groups, group);

      if (!target) {
        return prev;
      }

      const items = edit(target.items, title, (node) => ({
        ...node,
        isActive: !node.isActive,
      }));

      if (items === target.items) {
        return prev;
      }

      return {
        groups: prev.groups.map((g) =>
          g.title === group ? { ...g, items } : g,
        ),
      };
    });
  },
});
