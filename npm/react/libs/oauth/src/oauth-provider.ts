import { type AuthProvider } from '@axiomframework/react-core';
import { BaseAuthFlow } from './base-auth-flow';
import { PasswordAuthFlow } from './password-auth-flow';
import { CodeAuthFlow } from './code-auth-flow';

let instance: BaseAuthFlow | null = null;

export const oAuthProvider: AuthProvider = {
  get() {
    if (instance === undefined || instance === null) {
      throw new Error(`${typeof instance} could not provided`);
    }

    return instance;
  },
  provide(options) {
    if (instance) {
      return;
    }

    instance =
      options.flow === 'code'
        ? new CodeAuthFlow(options)
        : new PasswordAuthFlow(options);
  },
};
