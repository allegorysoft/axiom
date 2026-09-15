import type { JSX } from 'react';

export type Nav = {
  title: string;
  url?: string;
  icon?: JSX.Element;
  badge?: string;
  permission?: string | null;
  isActive?: boolean;
  children?: Nav[];
};

export type NavGroup = {
  title: string;
  isActive: boolean;
  items: Nav[];
};

const DEFAULT_KEY = 'Default';

export class NavManager {
  constructor(public groups: NavGroup[] = []) {
    this.addGroup(DEFAULT_KEY);
  }

  getGroup(title = DEFAULT_KEY) {
    return this.groups.find((group) => group.title === title);
  }

  addGroup(title: string) {
    const existing = this.getGroup(title);
    if (existing) {
      return existing;
    }

    const group: NavGroup = { title, isActive: false, items: [] };
    this.groups = [...this.groups, group];
    return group;
  }

  removeGroup(title: string) {
    this.groups = this.groups.filter((group) => group.title !== title);
  }

  get(title: string, group = DEFAULT_KEY) {
    return find(this.getGroup(group)?.items ?? [], title);
  }

  add(item: Nav, group = DEFAULT_KEY, parentTitle?: string) {
    this.mutate(group, (items) =>
      parentTitle
        ? this.appendToParent(items, parentTitle, item)
        : this.appendToRoot(items, item),
    );
  }

  protected appendToRoot(items: Nav[], item: Nav): Nav[] {
    return [...items, item];
  }

  protected appendToParent(
    items: Nav[],
    parentTitle: string,
    item: Nav,
  ): Nav[] {
    return edit(items, parentTitle, (parent) => ({
      ...parent,
      children: [...(parent.children ?? []), item],
    }));
  }

  remove(title: string, group = DEFAULT_KEY) {
    this.mutate(group, (items) => edit(items, title, () => undefined));
  }

  update(
    title: string,
    patch: Partial<Nav> | ((node: Nav) => Partial<Nav>),
    group = DEFAULT_KEY,
  ) {
    this.mutate(group, (items) =>
      edit(items, title, (node) => ({
        ...node,
        ...(typeof patch === 'function' ? patch(node) : patch),
      })),
    );
  }

  toggle(title: string, group = DEFAULT_KEY) {
    this.mutate(group, (items) =>
      edit(items, title, (node) => ({ ...node, isActive: !node.isActive })),
    );
  }

  protected mutate(group: string, fn: (items: Nav[]) => Nav[]) {
    const target = this.getGroup(group);

    if (!target) {
      this.groups = [
        ...this.groups,
        { title: group, isActive: false, items: fn([]) },
      ];
      return;
    }

    this.groups = this.groups.map((g) =>
      g.title === group ? { ...g, items: fn(g.items) } : g,
    );
  }
}

function find(items: Nav[], title: string): Nav | undefined {
  for (const item of items) {
    if (item.title === title) {
      return item;
    }

    if (item.children) {
      const child = find(item.children, title);
      if (child) {
        return child;
      }
    }
  }

  return undefined;
}

function edit(
  items: Nav[],
  title: string,
  fn: (node: Nav) => Nav | undefined,
): Nav[] {
  const result: Nav[] = [];

  for (const item of items) {
    if (item.title === title) {
      const next = fn(item);
      if (next) {
        result.push(next);
      }
      continue;
    }

    if (item.children) {
      result.push({ ...item, children: edit(item.children, title, fn) });
      continue;
    }

    result.push(item);
  }

  return result;
}

export const AxiomNavManager = new NavManager();
