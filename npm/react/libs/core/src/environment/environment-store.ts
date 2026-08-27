import { createStore } from '../store/axiom-store';
import type { OAuth } from '../models/oauth';
import type {
  Environment,
  Endpoint,
  EnvironmentState,
  EnvironmentStore,
} from './environment';
import { isDevMode } from '../utils/is-dev-mode';

const initialState: EnvironmentState = { environment: undefined };
const baseStore = createStore<EnvironmentState>(initialState);

export const environmentStore: EnvironmentStore = Object.assign(baseStore, {
  setEnvironment(environment: Environment | undefined): void {
    baseStore.set(() => ({ environment }));
  },

  patchEndpoints(endpoints: Record<string, Endpoint>): void {
    const environment = getEnvironmentOrWarn('patchEndpoints');
    if (!environment) {
      return;
    }

    baseStore.set(() => ({
      environment: {
        ...environment,
        endpoints: {
          ...environment.endpoints,
          ...endpoints,
        },
      },
    }));
  },

  patchOAuth(oauth: Partial<OAuth>): void {
    const environment = getEnvironmentOrWarn('patchOAuth');
    if (!environment) {
      return;
    }

    baseStore.set(() => ({
      environment: {
        ...environment,
        oauth: {
          ...environment.oauth,
          ...oauth,
        },
      },
    }));
  },

  reset(): void {
    baseStore.set(() => initialState);
  },
});

function getEnvironmentOrWarn(
  action: 'patchEndpoints' | 'patchOAuth',
): Environment | undefined {
  const { environment } = baseStore.get();

  if (!environment && isDevMode()) {
    console.warn(
      `[Axiom] ${action} called before the environment was initialised.`,
    );
  }

  return environment;
}
