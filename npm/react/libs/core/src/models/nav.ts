export type NavItem = {
  title: string;
  url?: string;
  icon?: React.JSX.Element;
  isActive?: boolean;
  badge?: string;
  children?: NavItem[];
};

export type NavGroup = {
  title: string;
  isActive: boolean;
  items: NavItem[];
};
