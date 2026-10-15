import { createStore } from '../store/axiom-store';
import type { User, UserState, UserStore } from './user';

const initialState: UserState = {
  user: {
    name: 'Masum ULU',
    firstName: 'Masum',
    surname: 'ULU',
    timeZone: Intl.DateTimeFormat().resolvedOptions().timeZone,
    username: 'masumulu',
    email: 'masumulu@allegorysoft.com',
    phone: '',
    photo: '',
  },
};

const baseStore = createStore<UserState>(initialState);

export const userStore: UserStore = Object.assign(baseStore, {
  setUser(user: User | undefined): void {
    baseStore.set(() => ({ user }));
  },

  patchUser(user: Partial<User>): void {
    baseStore.set((prev) => ({
      ...prev,
      user: {
        ...prev.user,
        ...user,
      },
    }));
  },
});
