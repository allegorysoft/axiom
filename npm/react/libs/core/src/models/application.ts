import { ApplicationInitializer } from './initializer';

export type Platform = 'server' | 'client';

export type ApplicationContext = {
  platform: Platform;
};

export type AxiomApplicationOptions = {
  initializers?: ApplicationInitializer[];
};
