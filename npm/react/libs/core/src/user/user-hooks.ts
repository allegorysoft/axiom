import { createStoreHook } from '../store/axiom-store';
import { userStore } from './user-store';

export const useUser = createStoreHook(userStore);
