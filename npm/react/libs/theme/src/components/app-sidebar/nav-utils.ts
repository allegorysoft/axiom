import type { Nav, NavGroup } from '@axiomframework/react-core';

export function isBranchActive(
  item: Nav,
  pathname: string,
  isRoot = true,
): boolean {
  const candidates = resolveCandidates(pathname, isRoot);
  const url = item.url ? normalize(item.url) : '';

  if (item.url && candidates.some((c) => normalize(c) === url)) {
    return true;
  }

  return (
    item.children?.some((child) =>
      isBranchActive(child, candidates[0], false),
    ) ?? false
  );
}

export function findStack(groups: NavGroup[], pathname: string): Nav[] {
  const candidates = resolveCandidates(pathname, true);

  for (const group of groups) {
    for (const candidate of candidates) {
      const stack = findStackInNodes(group.children, normalize(candidate));

      if (stack) {
        return stack;
      }
    }
  }

  return [];
}

function findStackInNodes(nodes: Nav[], pathname: string): Nav[] | null {
  for (const node of nodes) {
    if (!subtreeMatches(node, pathname)) {
      continue;
    }

    if (node.mode === 'switch-panel') {
      const nested: Nav[] = [
        ...(node.groups ? findStack(node.groups, pathname) : []),
        ...(node.children
          ? (findStackInNodes(node.children, pathname) ?? [])
          : []),
      ];
      return [node, ...nested];
    }

    if (node.children) {
      const nested = findStackInNodes(node.children, pathname);

      if (nested) {
        return nested;
      }
    }
  }

  return null;
}

function subtreeMatches(node: Nav, pathname: string): boolean {
  if (node.url === pathname) {
    return true;
  }

  if (node.children?.some((c) => subtreeMatches(c, pathname))) {
    return true;
  }

  return (
    node.groups?.some((g) =>
      g.children.some((c) => subtreeMatches(c, pathname)),
    ) ?? false
  );
}

function resolveCandidates(pathname: string, isRoot: boolean): string[] {
  if (!isRoot) {
    return [pathname];
  }

  const BASE_URL = import.meta.env.BASE_URL;

  if (BASE_URL.length === 0 || BASE_URL === '/') {
    return [pathname];
  }

  const stripped = sliceGroupNameFromPathName(BASE_URL, pathname);

  if (stripped === pathname) {
    return [pathname];
  }

  return [stripped, pathname];
}

function sliceGroupNameFromPathName(
  groupName: string,
  pathname: string,
): string {
  const base = `/${groupName.replace(/^\/+|\/+$/g, '')}`;

  if (base === '/') {
    return pathname;
  }

  if (pathname === base) {
    return '/';
  }

  if (pathname.startsWith(`${base}/`)) {
    return pathname.slice(base.length);
  }

  return pathname;
}

function normalize(url: string): string {
  return url.replace(/\/+$/, '') || '/';
}
