import { createStore } from '../store/axiom-store';
import type { User } from './user';

export const userStore = createStore<User>({
  name: 'Masum ULU',
  firstName: 'Masum',
  surname: 'ULU',
  timeZone: Intl.DateTimeFormat().resolvedOptions().timeZone,
  username: 'masumulu',
  email: 'masumulu@allegorysoft.com',
  phone: '',
  photo: '',
});
