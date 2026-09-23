import { createStore, createStoreHook } from '@axiomframework/react-core';

export const userProfileStore = createStore({
  name: 'Masum ULU',
  firstName: 'Masum',
  surname: 'ULU',
  timeZone: Intl.DateTimeFormat().resolvedOptions().timeZone,
  username: 'masumulu',
  email: 'masumulu@allegorysoft.com',
  phone: '',
  photo: '',
});

export const useUserProfile = createStoreHook(userProfileStore);
