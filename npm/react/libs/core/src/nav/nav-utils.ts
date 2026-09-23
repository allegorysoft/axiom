import type { Nav, NavGroup } from './nav';

export function selectGroup(groups: NavGroup[], title: string): NavGroup | undefined {
  return groups.find((group) => group.title === title);
}

export function find(items: Nav[], title: string): Nav | undefined {
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

export function edit(
  items: Nav[],
  title: string,
  fn: (node: Nav) => Nav | undefined,
): Nav[] {
  let changed = false;
  const result: Nav[] = [];

  for (const item of items) {
    if (item.title === title) {
      const next = fn(item);

      if (!next) {
        changed = true;
        continue;
      }

      if (next !== item) {
        changed = true;
      }

      result.push(next);
      continue;
    }

    if (item.children) {
      const children = edit(item.children, title, fn);

      if (children !== item.children) {
        changed = true;
        result.push({ ...item, children });
      } else {
        result.push(item);
      }

      continue;
    }

    result.push(item);
  }

  return changed ? result : items;
}
