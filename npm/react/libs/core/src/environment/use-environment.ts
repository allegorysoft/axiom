import { createStoreHook } from '../store/axiom-store';
import { environmentStore } from './environment-store';

export const useEnvironment = createStoreHook(environmentStore);
