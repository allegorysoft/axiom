import type { AxiomStore } from '../models/common';
import type { OAuth } from '../models/oauth';

export interface Environment {
  production: boolean;
  endpoints: Record<string, Endpoint>;
  oauth: OAuth;
}

export interface Endpoint {
  url: string;
}

export interface EnvironmentState {
  environment?: Environment;
}

export interface EnvironmentStore extends AxiomStore<EnvironmentState> {
  setEnvironment(environment: Environment | undefined): void;
  patchEndpoints(endpoints: Record<string, Endpoint>): void;
  patchOAuth(oauth: Partial<OAuth>): void;
  reset(): void;
}
