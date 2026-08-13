// oauth

import { type AuthProvider } from '@axiomframework/react-core';
import { PasswordAuthFlow } from './password-auth-flow';

export const oAuthProvider: AuthProvider = (options) => {
  if (options.flow === 'code') {
    //Code flow implementation
  }

  return new PasswordAuthFlow(options);
};
