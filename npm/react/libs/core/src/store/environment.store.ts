import { create } from 'zustand';
import type { OAuth } from '../models/oauth';
import type { Environment, Endpoint } from '../models/environment';

type EnvironmentState = { environment?: Environment };

type EnvironmentActions = {
  setEnvironment: (environment: Environment) => void;
  patchEndpoints: (endpoints: Record<string, Endpoint>) => void;
  patchOAuth: (oauth: Partial<OAuth>) => void;
  reset: () => void;
};

export const useEnvironmentStore = create<
  EnvironmentState & EnvironmentActions
>()((set) => ({
  environment: undefined,
  setEnvironment: (environment) => set({ environment }),

  patchEndpoints: (endpoints) =>
    set((state) => {
      const env = getEnvironmentOrWarn(state, 'patchEndpoints');
      if (!env) return state;

      return {
        environment: {
          ...env,
          endpoints: { ...env.endpoints, ...endpoints },
        },
      };
    }),

  patchOAuth: (oauth) =>
    set((state) => {
      const env = getEnvironmentOrWarn(state, 'patchOAuth');
      if (!env) return state;

      return {
        environment: {
          ...env,
          oauth: { ...env.oauth, ...oauth },
        },
      };
    }),

  reset: () => set({ environment: undefined }),
}));

function getEnvironmentOrWarn(
  state: EnvironmentState,
  action: keyof EnvironmentActions,
): Environment | undefined {
  if (!state.environment && !import.meta.env.PROD) {
    console.warn(
      `[axiom] ${action} called before the environment was initialised.`,
    );
  }

  return state.environment;
}
