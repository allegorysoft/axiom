import {
  type Nav,
  type NavGroup,
  DEFAULT_MENU_GROUP,
  withGroupName,
} from '@axiomframework/react-core';

export function findGroupPath(groups: NavGroup[], url: string): Nav[] {
  for (const group of groups) {
    const path = findNavPath(group.children, url);
    if (path.length) {
      return group.title && group.title !== DEFAULT_MENU_GROUP
        ? [{ title: group.title }, ...path]
        : path;
    }
  }

  return [];
}

export function findNavPath(items: Nav[], url: string): Nav[] {
  const normalize = (value: string) =>
    withGroupName(value.split(/[?#]/)[0]).replace(/\/+$/, '') || '/';

  for (const item of items) {
    const children = findNavPath(item.children ?? [], url);
    const nested = children.length
      ? children
      : findGroupPath(item.groups ?? [], url);

    if (nested.length) {
      return [item, ...nested];
    }

    if (item.url && normalize(item.url) === normalize(url)) {
      return [item];
    }
  }

  return [];
}
