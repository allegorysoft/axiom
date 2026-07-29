import { ApplicationInitializer } from './initializer';

export type Platform = 'server' | 'client';

export type AxiomApplicationOptions = {
  initializers?: ApplicationInitializer[];
};
