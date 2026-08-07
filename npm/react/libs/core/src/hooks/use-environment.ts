import { createStoreHook } from '../store/axiom-store';
import { environmentStore } from '../store/environment-store';

export const useEnvironment = createStoreHook(environmentStore);
