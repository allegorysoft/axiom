export type Nav = {
  title: string;
  url?: string;
  icon?: React.JSX.Element;
  isActive?: boolean;
  badge?: string;
  children?: Nav[];
};

export type NavGroup = {
  title: string;
  isActive: boolean;
  items: Nav[];
};
