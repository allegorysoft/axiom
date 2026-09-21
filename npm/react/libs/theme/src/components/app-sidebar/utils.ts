import { Nav } from '@axiomframework/react-core';

export function isBranchActive(item: Nav, pathname: string): boolean {
  if (item.url && item.url === pathname) {
    return true;
  }

  return (
    item.children?.some((child) => isBranchActive(child, pathname)) ?? false
  );
}
