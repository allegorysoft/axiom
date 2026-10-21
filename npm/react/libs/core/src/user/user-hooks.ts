import { createStoreHook } from '../store/axiom-store';
import type { User } from './user';
import { userStore } from './user-store';

export const useUserStore = createStoreHook(userStore);

export function useUser(): User | null {
  return useUserStore((s) => s.user);
}
