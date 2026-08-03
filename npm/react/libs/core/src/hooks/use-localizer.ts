import { createStoreHook } from '../store/axiom-store';
import { localizerStore } from '../store/localizer-store';

export const useLocalizer = createStoreHook(localizerStore);
