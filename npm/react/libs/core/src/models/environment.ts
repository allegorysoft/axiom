import type { OAuth } from './oauth';

export interface Environment {
  production: boolean;
  endpoints: Record<string, Endpoint>;
  oauth: OAuth;
}

export interface Endpoint {
  url: string;
}
