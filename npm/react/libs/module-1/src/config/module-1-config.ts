import { NavItemsOptions, provideNavItems } from './provide-nav-items';

type Module1Options = {
  navItems?: NavItemsOptions;
};

export function configureModule1(options?: Module1Options) {
  provideNavItems(options?.navItems);
}
