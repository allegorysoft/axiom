import type { MenuGroup, MenuNode } from './menu';

export function findGroup<T>(
  groups: MenuGroup<T>[],
  title: string,
): MenuGroup<T> | undefined {
  return groups.find((group) => group.title === title);
}

export function find<T extends MenuNode<T>>(
  items: T[],
  title: string,
): T | undefined {
  for (const item of items) {
    if (item.title === title) {
      return item;
    }

    if (item.children) {
      const found = find(item.children, title);

      if (found) {
        return found;
      }
    }
  }

  return undefined;
}

export function edit<T extends MenuNode<T>>(
  items: T[],
  title: string,
  update: (item: T) => T | undefined,
): T[] {
  let changed = false;

  const result = items.flatMap((item) => {
    if (item.title === title) {
      const next = update(item);

      if (!next) {
        changed = true;
        return [];
      }

      changed ||= next !== item;
      return [next];
    }

    if (!item.children) {
      return [item];
    }

    const children = edit(item.children, title, update);

    if (children === item.children) {
      return [item];
    }

    changed = true;
    return [{ ...item, children }];
  });

  return changed ? result : items;
}
