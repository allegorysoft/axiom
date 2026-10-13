import { provideInitializers } from '../initializer/registry';
import { seedLocalModulesFromEnvironment } from './nav-utils';

export type NavOptions = {
  skipLocalModuleSeed?: boolean;
};
export function configureNav(options?: NavOptions) {
  if (!options?.skipLocalModuleSeed) {
    provideInitializers({ configure: seedLocalModulesFromEnvironment });
  }
}
