import type { Nav, NavGroup } from '@axiomframework/react-core';

export function isBranchActive(item: Nav, pathname: string): boolean {
  if (item.url && item.url === pathname) {
    return true;
  }

  return (
    item.children?.some((child) => isBranchActive(child, pathname)) ?? false
  );
}

export function findStack(groups: NavGroup[], pathname: string): Nav[] {
  for (const group of groups) {
    const stack = findStackInNodes(group.children, pathname);
    if (stack) return stack;
  }
  return [];
}

function findStackInNodes(nodes: Nav[], pathname: string): Nav[] | null {
  for (const node of nodes) {
    if (!subtreeMatches(node, pathname)) continue;

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
      if (nested) return nested;
    }
  }
  return null;
}

function subtreeMatches(node: Nav, pathname: string): boolean {
  if (node.url === pathname) return true;
  if (node.children?.some((c) => subtreeMatches(c, pathname))) return true;
  if (
    node.groups?.some((g) =>
      g.children.some((c) => subtreeMatches(c, pathname)),
    )
  ) {
    return true;
  }
  return false;
}
