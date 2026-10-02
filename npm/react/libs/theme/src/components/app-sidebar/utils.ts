import {
  type Nav,
  type NavGroup,
  DEFAULT_MENU_GROUP,
} from '@axiomframework/react-core';

export function isBranchActive(item: Nav, pathname: string): boolean {
  if (item.url && item.url === pathname) {
    return true;
  }

  return (
    item.children?.some((child) => isBranchActive(child, pathname)) ?? false
  );
}

type SplitGroups = {
  defaultGroup: NavGroup | null;
  otherGroups: NavGroup[];
};

export function splitDefaultGroup(groups: NavGroup[] = []): SplitGroups {
  const index = groups.findIndex((g) => g.title === DEFAULT_MENU_GROUP);

  if (index === -1) {
    return { defaultGroup: null, otherGroups: groups };
  }

  return {
    defaultGroup: groups[index],
    otherGroups: groups.filter((_, i) => i !== index),
  };
}
