import { provideProfileTabs } from './tab-provider';
import { provideProfileUserActions } from './user-actions-provider';

export function configureProfile() {
  provideProfileUserActions();
  provideProfileTabs();
}
