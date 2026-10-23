import { createActionStore } from './action-store';
import { createActionHooks } from './action-hooks';

export const userActionStore = createActionStore();

export const {
  useActionStore: useUserActionStore,
  useActionGroups: useUserActionGroups,
  useActionGroup: useUserActionGroup,
  useActionItems: useUserActionItems,
} = createActionHooks(userActionStore);
